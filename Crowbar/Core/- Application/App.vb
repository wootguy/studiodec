Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.IO
Imports System.Text

Public Class App
	Implements IDisposable

#Region "Create and Destroy"

	Public Sub New()
		Me.IsDisposed = False

		'NOTE: To use a particular culture's NumberFormat that doesn't change with user settings, 
		'      must use this constructor with False as second param.
		Me.theInternalCultureInfo = New CultureInfo("en-US", False)
		Me.theInternalNumberFormat = Me.theInternalCultureInfo.NumberFormat

		Me.theSmdFilesWritten = New List(Of String)()
	End Sub

#Region "IDisposable Support"

	Public Sub Dispose() Implements IDisposable.Dispose
		' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) below.
		Dispose(True)
		GC.SuppressFinalize(Me)
	End Sub

	Protected Overridable Sub Dispose(ByVal disposing As Boolean)
		Me.IsDisposed = True
	End Sub

	'Protected Overrides Sub Finalize()
	'	' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
	'	Dispose(False)
	'	MyBase.Finalize()
	'End Sub

#End Region

#End Region

#Region "Properties"

	Public ReadOnly Property Settings() As AppSettings
		Get
			Return Me.theSettings
		End Get
	End Property

	Public ReadOnly Property CommandLineOption_Settings_IsEnabled() As Boolean
		Get
			Return Me.theCommandLineOption_Settings_IsEnabled
		End Get
	End Property

	Public ReadOnly Property ErrorPathFileName() As String
		Get
			Return Path.Combine(Me.GetCustomDataPath(), Me.ErrorFileName)
		End Get
	End Property

	Public ReadOnly Property Decompiler() As Decompiler
		Get
			Return Me.theDecompiler
		End Get
	End Property

	'Public ReadOnly Property Viewer() As Viewer
	'	Get
	'		Return Me.theModelViewer
	'	End Get
	'End Property

	'Public Property ModelRelativePathFileName() As String
	'	Get
	'		Return Me.theModelRelativePathFileName
	'	End Get
	'	Set(ByVal value As String)
	'		Me.theModelRelativePathFileName = value
	'	End Set
	'End Property

	Public ReadOnly Property InternalCultureInfo() As CultureInfo
		Get
			Return Me.theInternalCultureInfo
		End Get
	End Property

	Public ReadOnly Property InternalNumberFormat() As NumberFormatInfo
		Get
			Return Me.theInternalNumberFormat
		End Get
	End Property

	Public Property SmdFileNames() As List(Of String)
		Get
			Return Me.theSmdFilesWritten
		End Get
		Set(ByVal value As List(Of String))
			Me.theSmdFilesWritten = value
		End Set
	End Property

#End Region

#Region "Methods"

	Public Function CommandLineValueIsAnAppSetting(ByVal commandLineValue As String) As Boolean
		Return commandLineValue.StartsWith(App.SettingsParameter)
	End Function

	Public Function GetDebugPath(ByVal outputPath As String, ByVal modelName As String) As String
		'Dim logsPath As String

		'logsPath = Path.Combine(outputPath, modelName + "_" + App.LogsSubFolderName)

		'Return logsPath
		Return outputPath
	End Function

	'TODO: [GetCustomDataPath] Have location option where custom data and settings is saved.
	Public Function GetCustomDataPath() As String
		Dim customDataPath As String
		'Dim appDataPath As String

		'' If the settings file exists in the app's Data folder, then load it.
		'appDataPath = Me.GetAppDataPath()
		'If appDataPath <> "" Then
		'	customDataPath = appDataPath
		'Else
		'NOTE: Use "standard Windows location for app data".
		'NOTE: Using Path.Combine in case theStartupFolder is a root folder, like "C:\".
		customDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZeqMacaw")
		customDataPath += Path.DirectorySeparatorChar
		'customDataPath += "Crowbar"
		customDataPath += My.Application.Info.ProductName
		customDataPath += " "
		customDataPath += My.Application.Info.Version.ToString(2)

		FileManager.CreatePath(customDataPath)
		'End If

		Return customDataPath
	End Function

#End Region

#Region "Private Methods"

	Public Sub CreateAppSettings()
		Me.theSettings = New AppSettings()
	End Sub

	'Private Function GetAppDataPath() As String
	'	Dim appDataPath As String
	'	Dim appDataPathFileName As String

	'	appDataPath = Path.Combine(Me.theAppPath, App.theDataFolderName)
	'	appDataPathFileName = Path.Combine(appDataPath, App.theAppSettingsFileName)

	'	If File.Exists(appDataPathFileName) Then
	'		Return appDataPath
	'	Else
	'		Return ""
	'	End If
	'End Function

	Private Sub WriteResourceToFileIfDifferent(ByVal dataResource As Byte(), ByVal pathFileName As String)
		Try
			Dim isDifferentOrNotExist As Boolean = True
			If File.Exists(pathFileName) Then
				Dim resourceHash() As Byte
				Dim sha As New Security.Cryptography.SHA512Managed()
				resourceHash = sha.ComputeHash(dataResource)

				Dim fileStream As FileStream = File.Open(pathFileName, FileMode.Open)
				Dim fileHash() As Byte = sha.ComputeHash(fileStream)
				fileStream.Close()

				isDifferentOrNotExist = False
				For x As Integer = 0 To resourceHash.Length - 1
					If resourceHash(x) <> fileHash(x) Then
						isDifferentOrNotExist = True
						Exit For
					End If
				Next
			End If

			If isDifferentOrNotExist Then
				File.WriteAllBytes(pathFileName, dataResource)
			End If
		Catch ex As Exception
			Console.WriteLine("EXCEPTION: " + ex.Message)
			'Throw New Exception(ex.Message, ex.InnerException)
			Exit Sub
		Finally
		End Try
	End Sub

	Public Function GetHeaderComment() As String
		Return ""
	End Function

	Public Function GetProcessedPathFileName(ByVal pathFileName As String) As String
		Dim result As String

		result = pathFileName

		Return result
	End Function

#End Region

#Region "Data"

	Private IsDisposed As Boolean

	Private theInternalCultureInfo As CultureInfo
	Private theInternalNumberFormat As NumberFormatInfo

	Private theSettings As AppSettings
	'NOTE: Use slash at start to avoid confusing with a pathFileName that Windows Explorer might use with auto-open.
	Public Const SettingsParameter As String = "/settings="
	Private theCommandLineOption_Settings_IsEnabled As Boolean

	' Location of the exe.
	Private theAppPath As String

	Public Const AnimsSubFolderName As String = "anims"
	Public Const LogsSubFolderName As String = "logs"

	Private ErrorFileName As String = "unhandled_exception_error.txt"

	Private theDecompiler As Decompiler
	'Private theModelViewer As Viewer
	Private theModelRelativePathFileName As String

	Private theSmdFilesWritten As List(Of String)

#End Region

End Class
