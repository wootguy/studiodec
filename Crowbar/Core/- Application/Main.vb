Imports System.IO
Imports Steamworks

Module Main

	' Entry point of application.
	Public Function Main() As Integer
		'' Create a job with JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE flag, so that all processes 
		''	(e.g. HLMV called by Crowbar) associated with the job 
		''	terminate when the last handle to the job is closed.
		'' From MSDN: By default, processes created using CreateProcess by a process associated with a job 
		''	are associated with the job.
		'TheJob = New WindowsJob()
		'TheJob.AddProcess(Process.GetCurrentProcess().Handle())

		Dim args() As String = Environment.GetCommandLineArgs()

		Try
			TheApp = New App()
			TheApp.Init()

			Dim input As String = args(1)
			Dim output As String = args(2)

			TheApp.Settings.DecompileMdlPathFileName = input
			TheApp.Settings.DecompileMode = InputOptions.File  ' or Folder / FolderRecursion
			TheApp.Settings.DecompileUseNonValveUvConversionIsChecked = True

			' Create the object that owns Decompile()
			Dim decompiler As New Decompiler()
			decompiler.theOutputPath = output
			Dim result As StatusMessage = decompiler.Decompile()

			Console.WriteLine("Result: " & result.ToString())

			TheApp.Dispose()
			Return 0

		Catch ex As Exception
			Console.WriteLine("Error: " & ex.Message)
			Return 1
		End Try

		Console.WriteLine("Usage: studiodec .\model.mdl output\path")
		Return 0
	End Function

	Private Sub StartupNextInstanceEventHandler(ByVal sender As Object, ByVal e As SingleInstanceEventArgs)
		If e.MainForm.WindowState = FormWindowState.Minimized Then
			e.MainForm.WindowState = FormWindowState.Normal
		End If
		e.MainForm.Activate()
		CType(e.MainForm, MainForm).Startup(e.CommandLine)
	End Sub

	Private Function ResolveAssemblies(sender As Object, e As System.ResolveEventArgs) As Reflection.Assembly
		Dim desiredAssembly As Reflection.AssemblyName = New Reflection.AssemblyName(e.Name)
		'If desiredAssembly.Name = "SevenZipSharp" Then
		'	Return Reflection.Assembly.Load(My.Resources.SevenZipSharp)
		'ElseIf desiredAssembly.Name = "Steamworks.NET" Then
		'	Return Reflection.Assembly.Load(My.Resources.Steamworks_NET)
		'Else
		'	Return Nothing
		'End If
		If desiredAssembly.Name = "Steamworks.NET" Then
			Return Reflection.Assembly.Load(My.Resources.Steamworks_NET)
		Else
			Return Nothing
		End If
	End Function

	'Public TheJob As WindowsJob
	Public TheApp As App

End Module
