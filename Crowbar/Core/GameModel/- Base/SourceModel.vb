Imports System.IO
Imports System.ComponentModel

Public MustInherit Class SourceModel

#Region "Shared"

	Public Shared Function Create(ByVal mdlPathFileName As String, ByVal overrideVersion As SupportedMdlVersion, ByRef version As Integer) As SourceModel
		Dim model As SourceModel = Nothing

		Try
			version = SourceModel.GetVersion(mdlPathFileName)

			If version = 10 Then
				model = New SourceModel10(mdlPathFileName, version)
			Else
				' Version not implemented.
				model = Nothing
			End If
		Catch ex As Exception
			Throw
		End Try

		Return model
	End Function

	Private Shared Function GetVersion(mdlPathFileName As String) As Integer
		Dim version As Integer
		Dim inputFileStream As FileStream
		Dim inputFileReader As BinaryReader

		version = -1
		inputFileStream = Nothing
		inputFileReader = Nothing
		Try
			inputFileStream = New FileStream(mdlPathFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
			If inputFileStream IsNot Nothing Then
				Try
					'NOTE: Important to set System.Text.Encoding.ASCII so that ReadChars() only reads in one byte per Char.
					inputFileReader = New BinaryReader(inputFileStream, System.Text.Encoding.ASCII)

					Dim id As String
					id = inputFileReader.ReadChars(4)
					version = inputFileReader.ReadInt32()
					If id = "MDLZ" Then
						If version <> 14 Then
							Throw New FormatException("File with header ID (first 4 bytes of file) of 'MDLZ' (without quotes) does not have expected MDL version of 14. MDL file is not a GoldSource- or Source-engine MDL file.")
						End If
					ElseIf id <> "IDST" Then
						Throw New FormatException("File does not have expected MDL header ID (first 4 bytes of file) of 'IDST' or 'MDLZ' (without quotes). MDL file is not a GoldSource- or Source-engine MDL file.")
					End If
				Catch ex As FormatException
					Throw
				Catch ex As Exception
					'Dim debug As Integer = 4242
					Throw
				Finally
					If inputFileReader IsNot Nothing Then
						inputFileReader.Close()
					End If
				End Try
			End If
		Catch ex As FormatException
			Throw
		Catch ex As Exception
			'Dim debug As Integer = 4242
			Throw
		Finally
			If inputFileStream IsNot Nothing Then
				inputFileStream.Close()
			End If
		End Try

		Return version
	End Function

	'Private Shared version_shared As Integer

#End Region

#Region "Creation and Destruction"

	Protected Sub New(ByVal mdlPathFileName As String, ByVal mdlVersion As Integer)
		Me.theMdlPathFileName = mdlPathFileName
		Me.theName = Path.GetFileNameWithoutExtension(mdlPathFileName)
		Me.theVersion = mdlVersion
	End Sub

#End Region

#Region "Properties - Model Data"

	Public ReadOnly Property ID() As String
		Get
			Return Me.theMdlFileDataGeneric.theID
		End Get
	End Property

	Public ReadOnly Property Version() As Integer
		Get
			Return Me.theVersion
		End Get
	End Property

	Public ReadOnly Property Name() As String
		Get
			Return Me.theName
		End Get
		'Set(ByVal value As String)
		'	Me.theName = value
		'End Set
	End Property

#End Region

#Region "Properties - File-Related"

	' The *Used properties should return whether the files are actually referred to by the MDL file.
	' For the PHY file and others that have no reference in the MDL file, simply return whether each file exists.

	Public Overridable ReadOnly Property SequenceGroupMdlFilesAreUsed As Boolean
		Get
			Return False
		End Get
	End Property

	Public Overridable ReadOnly Property TextureMdlFileIsUsed As Boolean
		Get
			Return False
		End Get
	End Property

#End Region

#Region "Properties - Data Query"

	Public Overridable ReadOnly Property HasTextureData As Boolean
		Get
			Return False
		End Get
	End Property

	Public Overridable ReadOnly Property HasMeshData As Boolean
		Get
			Return False
		End Get
	End Property

	Public Overridable ReadOnly Property HasBoneAnimationData As Boolean
		Get
			Return False
		End Get
	End Property
	Public Overridable ReadOnly Property HasTextureFileData As Boolean
		Get
			Return False
		End Get
	End Property

#End Region

#Region "Methods"

	Public Overridable Function ReadMdlFileHeader() As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		If Not File.Exists(Me.theMdlPathFileName) Then
			status = StatusMessage.ErrorRequiredMdlFileNotFound
		End If

		If status = StatusMessage.Success Then
			Me.ReadFile(Me.theMdlPathFileName, AddressOf Me.ReadMdlFileHeader_Internal)
		End If

		Return status
	End Function

	Public Overridable Function CheckForRequiredFiles() As FilesFoundFlags
		Dim status As AppEnums.FilesFoundFlags

		status = FilesFoundFlags.AllFilesFound

		Return status
	End Function

	Public Overridable Function ReadSequenceGroupMdlFiles() As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		status = StatusMessage.Error

		Return status
	End Function

	Public Overridable Function ReadTextureMdlFile() As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		status = StatusMessage.Error

		Return status
	End Function

	Public Overridable Function ReadMdlFile() As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		Try
			Me.ReadFile(Me.theMdlPathFileName, AddressOf Me.ReadMdlFile_Internal)
		Catch ex As Exception
			status = StatusMessage.Error
		End Try

		Return status
	End Function
	Public Overridable Function SetAllSmdPathFileNames() As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		Return status
	End Function

	Public Overridable Function WriteQcFile(ByVal qcPathFileName As String) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success
		Dim writeStatus As String

		Me.theQcPathFileName = qcPathFileName
		Me.NotifySourceModelProgress(ProgressOptions.WritingFileStarted, qcPathFileName)
		writeStatus = Me.WriteTextFile(qcPathFileName, AddressOf Me.WriteQcFile)
		If writeStatus = "Success" Then
			Me.NotifySourceModelProgress(ProgressOptions.WritingFileFinished, qcPathFileName)
		Else
			Me.NotifySourceModelProgress(ProgressOptions.WritingFileFailed, writeStatus)
		End If

		Return status
	End Function

	Public Overridable Function WriteReferenceMeshFiles(ByVal modelOutputPath As String) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		Return status
	End Function

	Public Overridable Function WriteMeshSmdFile(ByVal smdPathFileName As String, ByVal lodIndex As Integer, ByVal aModel As SourceMdlModel, ByVal bodyPartVertexIndexStart As Integer) As String
		Dim status As String = "Success"

		Return status
	End Function

	Public Overridable Function WriteBoneAnimationSmdFiles(ByVal modelOutputPath As String) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		Return status
	End Function

	Public Overridable Function WriteBoneAnimationSmdFile(ByVal smdPathFileName As String, ByVal aSequenceDesc As SourceMdlSequenceDescBase, ByVal anAnimationDesc As SourceMdlAnimationDescBase) As String
		Dim status As String = "Success"

		Return status
	End Function

	Public Overridable Function WriteTextureFiles(ByVal modelOutputPath As String) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		status = StatusMessage.Error

		Return status
	End Function
	Public Overridable Function GetTextureFileNames() As List(Of String)
		Return New List(Of String)()
	End Function

	Public Overridable Function GetSequenceInfo() As List(Of String)
		Return New List(Of String)()
	End Function

#End Region

#Region "Events"

	Public Event SourceModelProgress As SourceModelProgressEventHandler

#End Region

#Region "Protected Methods"

	Protected Overridable Sub ReadMdlFile_Internal()

	End Sub

	Protected Overridable Function ReadSequenceGroupMdlFile(ByVal pathFileName As String, ByVal sequenceGroupIndex As Integer) As AppEnums.StatusMessage
		Dim status As AppEnums.StatusMessage = StatusMessage.Success

		Dim inputFileStream As FileStream = Nothing
		Me.theInputFileReader = Nothing
		Try
			inputFileStream = New FileStream(pathFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
			If inputFileStream IsNot Nothing Then
				Try
					Me.theInputFileReader = New BinaryReader(inputFileStream, System.Text.Encoding.ASCII)

					ReadSequenceGroupMdlFile(sequenceGroupIndex)
				Catch ex As Exception
					Throw
				Finally
					If Me.theInputFileReader IsNot Nothing Then
						Me.theInputFileReader.Close()
					End If
				End Try
			End If
		Catch ex As Exception
			Throw
		Finally
			If inputFileStream IsNot Nothing Then
				inputFileStream.Close()
			End If
		End Try

		Return status
	End Function

	Protected Overridable Sub ReadSequenceGroupMdlFile(ByVal sequenceGroupIndex As Integer)

	End Sub

	Protected Overridable Sub ReadTextureMdlFile_Internal()

	End Sub

	Protected Overridable Sub WriteQcFile()

	End Sub

	Protected Overridable Sub WriteMeshSmdFile(ByVal lodIndex As Integer, ByVal aModel As SourceMdlModel, ByVal bodyPartVertexIndexStart As Integer)

	End Sub

	Protected Overridable Sub WriteBoneAnimationSmdFile(ByVal aSequenceDesc As SourceMdlSequenceDescBase, ByVal anAnimationDesc As SourceMdlAnimationDescBase)

	End Sub

	Protected Overridable Sub WriteTextureFile()

	End Sub

	Protected Overridable Sub ReadMdlFileHeader_Internal()

	End Sub

	Protected Overridable Sub ReadMdlFileForViewer_Internal()

	End Sub

	Protected Overridable Sub WriteMdlFileNameToMdlFile(ByVal internalMdlFileName As String)

	End Sub

	Protected Overridable Sub WriteAniFileNameToMdlFile(ByVal internalAniFileName As String)

	End Sub

	Protected Sub ReadFile(ByVal pathFileName As String, ByVal readFileAction As ReadFileDelegate)
		Dim inputFileStream As FileStream = Nothing
		Me.theInputFileReader = Nothing
		Try
			inputFileStream = New FileStream(pathFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
			If inputFileStream IsNot Nothing Then
				Try
					Me.theInputFileReader = New BinaryReader(inputFileStream, System.Text.Encoding.ASCII)

					readFileAction.Invoke()
				Catch ex As Exception
					Throw
				Finally
					If Me.theInputFileReader IsNot Nothing Then
						Me.theInputFileReader.Close()
					End If
				End Try
			End If
		Catch ex As Exception
			Throw
		Finally
			If inputFileStream IsNot Nothing Then
				inputFileStream.Close()
			End If
		End Try
	End Sub

	Protected Function WriteTextFile(ByVal outputPathFileName As String, ByVal writeTextFileAction As WriteTextFileDelegate) As String
		Dim status As String = "Success"

		Try
			Me.theOutputFileTextWriter = File.CreateText(outputPathFileName)

			writeTextFileAction.Invoke()
		Catch ex As PathTooLongException
			status = "ERROR: Crowbar tried to create """ + outputPathFileName + """ but the system gave this message: " + ex.Message
		Catch ex As Exception
			Dim debug As Integer = 4242
		Finally
			If Me.theOutputFileTextWriter IsNot Nothing Then
				Me.theOutputFileTextWriter.Flush()
				Me.theOutputFileTextWriter.Close()
			End If
		End Try

		Return status
	End Function

	Protected Sub NotifySourceModelProgress(ByVal progress As ProgressOptions, ByVal message As String)
		RaiseEvent SourceModelProgress(Me, New SourceModelProgressEventArgs(progress, message))
	End Sub

#End Region

#Region "Private Delegates"

	Public Delegate Sub SourceModelProgressEventHandler(ByVal sender As Object, ByVal e As SourceModelProgressEventArgs)

	Protected Delegate Sub ReadFileDelegate()
	Protected Delegate Sub WriteFileDelegate(ByVal value As String)
	Protected Delegate Sub WriteTextFileDelegate()

#End Region


#Region "Data"

	Protected theVersion As Integer
	Protected theName As String
	'Protected thePhysicsMeshSmdFileName As String

	Protected theMdlFileDataGeneric As SourceMdlFileDataBase
	Protected theAniFileDataGeneric As SourceFileData

	Protected theInputFileReader As BinaryReader
	Protected theOutputFileBinaryWriter As BinaryWriter
	Protected theOutputFileTextWriter As StreamWriter

	Protected theMdlPathFileName As String
	Protected theSequenceGroupMdlPathFileNames As List(Of String)
	Protected theTextureMdlPathFileName As String

	Protected theQcPathFileName As String

#End Region

End Class
