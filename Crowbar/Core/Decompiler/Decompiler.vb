Imports System.ComponentModel
Imports System.IO

Public Class Decompiler

#Region "Private Methods in Background Thread"

	Public Function Decompile(ByVal outputPath As String) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		Dim mdlPathFileName As String
		mdlPathFileName = TheApp.Settings.DecompileMdlPathFileName
		If File.Exists(mdlPathFileName) Then
			Me.theInputMdlPathName = FileManager.GetPath(mdlPathFileName)
		ElseIf Directory.Exists(mdlPathFileName) Then
			Me.theInputMdlPathName = mdlPathFileName
		End If

		Dim progressDescriptionText As String
		progressDescriptionText = "Decompiling: "

		Try
			If Me.theInputMdlPathName = "" Then
				'Can get here if mdlPathFileName exists, but only with parts of the path using "Length8.3" names.
				'Somehow when drag-dropping such a pathFileName, even though Windows shows full names in the path, Crowbar shows it with "Length8.3" names.
				progressDescriptionText += """" + mdlPathFileName + """"
				Me.UpdateProgressStart(progressDescriptionText + " ...")
				Me.UpdateProgress()
				Me.UpdateProgress(1, "ERROR: Failed because actual path is too long.")
				status = StatusMessage.Error
			Else
				progressDescriptionText += """" + mdlPathFileName + """"
				Me.UpdateProgressStart(progressDescriptionText + " ...")
				status = Me.DecompileOneModel(mdlPathFileName, outputPath)
			End If
		Catch ex As Exception
			status = StatusMessage.Error
		End Try

		Me.UpdateProgressStop("... " + progressDescriptionText + " finished.")

		Return status
	End Function

	Private Function DecompileOneModel(ByVal mdlPathFileName As String, ByVal outputPath As String) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		Try
			Dim mdlFileName As String
			Dim mdlRelativePathName As String
			Dim mdlRelativePathFileName As String
			mdlFileName = Path.GetFileName(mdlPathFileName)
			mdlRelativePathName = FileManager.GetRelativePathFileName(Me.theInputMdlPathName, FileManager.GetPath(mdlPathFileName))
			mdlRelativePathFileName = Path.Combine(mdlRelativePathName, mdlFileName)

			Dim modelName As String
			modelName = Path.GetFileNameWithoutExtension(mdlPathFileName)

			Me.theModelOutputPath = Path.Combine(outputPath, mdlRelativePathName)
			Me.theModelOutputPath = Path.GetFullPath(Me.theModelOutputPath)
			If TheApp.Settings.DecompileFolderForEachModelIsChecked Then
				Me.theModelOutputPath = Path.Combine(Me.theModelOutputPath, modelName)
			End If

			FileManager.CreatePath(Me.theModelOutputPath)

			Me.UpdateProgress()
			Me.UpdateProgress(1, "Decompiling """ + mdlRelativePathFileName + """ ...")

			Dim model As SourceModel = Nothing
			Dim version As Integer
			Try
				model = SourceModel.Create(mdlPathFileName, TheApp.Settings.DecompileOverrideMdlVersion, version)
				If model IsNot Nothing Then
					If TheApp.Settings.DecompileOverrideMdlVersion = SupportedMdlVersion.DoNotOverride Then
						Me.UpdateProgress(2, "Model version " + CStr(model.Version) + " detected.")
					Else
						Me.UpdateProgress(2, "Model version overridden to be " + CStr(model.Version) + ".")
					End If
				Else
					Me.UpdateProgress(2, "ERROR: Model version " + CStr(version) + " not supported.")
					Me.UpdateProgress(1, "... Decompiling """ + mdlRelativePathFileName + """ FAILED.")
					status = StatusMessage.Error
					Return status
				End If
			Catch ex As FormatException
				Me.UpdateProgress(2, ex.Message)
				Me.UpdateProgress(1, "... Decompiling """ + mdlRelativePathFileName + """ FAILED.")
				status = StatusMessage.Error
				Return status
			Catch ex As Exception
				Me.UpdateProgress(2, "Crowbar tried to read the MDL file but the system gave this message: " + ex.Message)
				Me.UpdateProgress(1, "... Decompiling """ + mdlRelativePathFileName + """ FAILED.")
				status = StatusMessage.Error
				Return status
			End Try

			Me.UpdateProgress(2, "Reading MDL file header ...")
			status = model.ReadMdlFileHeader()

			If status = StatusMessage.ErrorInvalidInternalMdlFileSize Then
				Me.UpdateProgress(3, "WARNING: The internally recorded file size is different than the actual file size. Some data might not decompile correctly.")
			End If
			Me.UpdateProgress(2, "... Reading MDL file header finished.")

			Me.UpdateProgress(2, "Checking for required files ...")
			Dim filesFoundFlags As AppEnums.FilesFoundFlags
			filesFoundFlags = model.CheckForRequiredFiles()

			If filesFoundFlags = AppEnums.FilesFoundFlags.ErrorRequiredSequenceGroupMdlFileNotFound Then
				Me.UpdateProgress(2, "ERROR: Sequence Group MDL file not found.")
				Return StatusMessage.ErrorRequiredSequenceGroupMdlFileNotFound
			ElseIf filesFoundFlags = AppEnums.FilesFoundFlags.ErrorRequiredTextureMdlFileNotFound Then
				Me.UpdateProgress(2, "ERROR: Texture MDL file not found.")
				Return StatusMessage.ErrorRequiredTextureMdlFileNotFound
			End If
			If filesFoundFlags = AppEnums.FilesFoundFlags.AllFilesFound Then
				Me.UpdateProgress(2, "... All required files found.")
			Else
				Me.UpdateProgress(2, "... Not all required files found, but decompiling available files.")
			End If

			Me.UpdateProgress(2, "Reading data ...")
			status = Me.ReadCompiledFiles(mdlPathFileName, model)
			If status = StatusMessage.ErrorRequiredMdlFileNotFound Then
				Me.UpdateProgress(1, "... Decompiling """ + mdlRelativePathFileName + """ stopped due to missing file.")
				Return status
			ElseIf status = StatusMessage.ErrorInvalidMdlFileId Then
				Me.UpdateProgress(1, "... Decompiling """ + mdlRelativePathFileName + """ stopped due to invalid file.")
				Return status
			ElseIf status = StatusMessage.Error Then
				Me.UpdateProgress(1, "... Decompiling """ + mdlRelativePathFileName + """ stopped due to error.")
				Return status
			Else
				Me.UpdateProgress(2, "... Reading data finished.")
			End If

			Me.UpdateProgress(2, "Writing data ...")
			Me.WriteDecompiledFiles(model)
			Me.UpdateProgress(2, "... Writing data finished.")
		Catch ex As Exception
			Dim debug As Integer = 4242
		End Try

		Return status
	End Function

	Private Function ReadCompiledFiles(ByVal mdlPathFileName As String, ByVal model As SourceModel) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		Me.UpdateProgress(3, "Reading MDL file ...")
		status = model.ReadMdlFile()
		If status = StatusMessage.Success Then
			Me.UpdateProgress(3, "... Reading MDL file finished.")
		ElseIf status = StatusMessage.Error Then
			Me.UpdateProgress(3, "... Reading MDL file FAILED. (Probably unexpected format.)")
			Return status
		End If

		If model.SequenceGroupMdlFilesAreUsed Then
			Me.UpdateProgress(3, "Reading sequence group MDL files ...")
			status = model.ReadSequenceGroupMdlFiles()
			If status = StatusMessage.Success Then
				Me.UpdateProgress(3, "... Reading sequence group MDL files finished.")
			ElseIf status = StatusMessage.Error Then
				Me.UpdateProgress(3, "... Reading sequence group MDL files FAILED. (Probably unexpected format.)")
			End If
		End If

		If model.TextureMdlFileIsUsed Then
			Me.UpdateProgress(3, "Reading texture MDL file ...")
			status = model.ReadTextureMdlFile()
			If status = StatusMessage.Success Then
				Me.UpdateProgress(3, "... Reading texture MDL file finished.")
			ElseIf status = StatusMessage.Error Then
				Me.UpdateProgress(3, "... Reading texture MDL file FAILED. (Probably unexpected format.)")
			End If
		End If

		Return status
	End Function

	'Private Function ProcessData(ByVal model As SourceModel) As AppEnums.StatusMessage
	'	Dim status As AppEnums.StatusMessage = StatusMessage.Success

	'	'TODO: Create all possible SMD file names before using them, so can handle any name collisions.
	'	'      Store mesh SMD file names in list in SourceMdlModel where the index is lodIndex.
	'	'      Store anim SMD file name in SourceMdlAnimationDesc48.

	'	Return status
	'End Function

	Private Function WriteDecompiledFiles(ByVal model As SourceModel) As AppEnums.StatusMessage
		TheApp.SmdFileNames.Clear()

		Me.WriteQcFile(model)
		Me.WriteReferenceMeshFiles(model)
		Me.WriteBoneAnimationFiles(model)
		Me.WriteTextureFiles(model)

		Return StatusMessage.Success
	End Function

	Private Function WriteQcFile(ByVal model As SourceModel) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		Me.UpdateProgress(3, "QC file: ")
		AddHandler model.SourceModelProgress, AddressOf Me.Model_SourceModelProgress

		Dim qcPathFileName As String
		qcPathFileName = Path.Combine(Me.theModelOutputPath, model.Name + ".qc")

		status = model.WriteQcFile(qcPathFileName)

		RemoveHandler model.SourceModelProgress, AddressOf Me.Model_SourceModelProgress

		Return status
	End Function

	Private Function WriteReferenceMeshFiles(ByVal model As SourceModel) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		If model.HasMeshData Then
			Me.UpdateProgress(3, "Reference mesh files: ")
			AddHandler model.SourceModelProgress, AddressOf Me.Model_SourceModelProgress

			status = model.WriteReferenceMeshFiles(Me.theModelOutputPath)

			RemoveHandler model.SourceModelProgress, AddressOf Me.Model_SourceModelProgress
		End If

		Return status
	End Function

	Private Function WriteBoneAnimationFiles(ByVal model As SourceModel) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		If model.HasBoneAnimationData Then
			Dim outputPath As String
			outputPath = Path.Combine(Me.theModelOutputPath, SourceFileNamesModule.GetAnimationSmdRelativePath(model.Name))
			If FileManager.PathExistsAfterTryToCreate(outputPath) Then
				Me.UpdateProgress(3, "Bone animation files: ")
				AddHandler model.SourceModelProgress, AddressOf Me.Model_SourceModelProgress

				status = model.WriteBoneAnimationSmdFiles(Me.theModelOutputPath)

				RemoveHandler model.SourceModelProgress, AddressOf Me.Model_SourceModelProgress
			Else
				Me.UpdateProgress(3, "WARNING: Unable to create """ + outputPath + """ where bone animation SMD files would be written.")
			End If
		End If

		Return status
	End Function

	Private Function WriteTextureFiles(ByVal model As SourceModel) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		If model.HasTextureFileData Then
			Me.UpdateProgress(3, "Texture files: ")
			AddHandler model.SourceModelProgress, AddressOf Me.Model_SourceModelProgress

			status = model.WriteTextureFiles(Me.theModelOutputPath)

			RemoveHandler model.SourceModelProgress, AddressOf Me.Model_SourceModelProgress
		End If

		Return status
	End Function

	Private Sub UpdateProgressStart(ByVal line As String)
		Me.UpdateProgressInternal(0, line)
	End Sub

	Private Sub UpdateProgressStop(ByVal line As String)
		Me.UpdateProgressInternal(100, vbCr + line)
	End Sub

	Private Sub UpdateProgress()
		Me.UpdateProgressInternal(1, "")
	End Sub

	Private Sub UpdateProgress(ByVal indentLevel As Integer, ByVal line As String)
		Dim indentedLine As String

		indentedLine = ""
		For i As Integer = 1 To indentLevel
			indentedLine += "  "
		Next
		indentedLine += line
		Me.UpdateProgressInternal(1, indentedLine)
	End Sub

	Private Sub UpdateProgressInternal(ByVal progressValue As Integer, ByVal line As String)
		Console.WriteLine(line)
	End Sub

#End Region

#Region "Event Handlers"

	Private Sub Model_SourceModelProgress(ByVal sender As Object, ByVal e As SourceModelProgressEventArgs)
		If e.Progress = ProgressOptions.WritingFileFailed Then
			Me.UpdateProgress(4, e.Message)
		ElseIf e.Progress = ProgressOptions.WritingFileFinished Then
			Dim pathFileName As String
			Dim fileName As String
			pathFileName = e.Message
			fileName = Path.GetFileName(pathFileName)
			Me.UpdateProgress(4, fileName)

			Dim model As SourceModel
			model = CType(sender, SourceModel)
		Else
			Dim progressUnhandled As Integer = 4242
		End If
	End Sub

#End Region

#Region "Data"

	Private theInputMdlPathName As String
	Private theModelOutputPath As String

#End Region

End Class
