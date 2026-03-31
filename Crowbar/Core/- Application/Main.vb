Imports System.IO

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
			TheApp.CreateAppSettings()

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

	'Public TheJob As WindowsJob
	Public TheApp As App

End Module
