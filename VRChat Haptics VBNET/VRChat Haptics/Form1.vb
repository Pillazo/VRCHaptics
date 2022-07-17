Imports System.ComponentModel 'Other misc imports
Imports System.Environment
Imports System.Diagnostics
Imports System.Net.Sockets

Public Class Form1

    Dim OSCR As New UdpClient(9001) 'OSC recieves on UDP, at port 9001 (as stated from VRC)
    Dim OSCep As System.Net.IPEndPoint 'Just defining somewhere to look for OSC
    Dim OSCP As String = "" 'Parameter name
    Dim OSCvalue As String = "" 'Place to save string for displaying
    Dim OSCPSaved As String = "" 'Parameter name if its unique
    Dim OSCValueSaved As String = "" 'Value for saving to
    Dim AppDatafolder As String = ""
    Dim avatarfiles As New List(Of String)

    Public NodeDeviceNames As New List(Of String)    'Name of Device for reference like 'head' or 'right arm' kinda thing
    Public NodeOutputs As New List(Of List(Of Boolean)) 'Output of device
    Public NodeDeviceConnection As New List(Of Integer) 'Connection timeout
    Public NodeForce As New List(Of List(Of Integer)) 'The intensity of buzzing for each node
    Public NodeNames As New List(Of List(Of String)) 'The parameter name coming from OSC
    Public DeviceIndex As Integer   'for sharing to the other dialog for device editing
    Public DeviceMethod As Integer = 0  'for sharing to the other dialog for device editing
    Dim DGVupdating As Boolean = False  'DGVNode ignores updates if DGVDevice is changing it
    Dim sendbytezzz As Byte()
    Dim testednodeD As Integer 'Testing device number
    Dim testednodeN As Integer 'Testing node number

    Dim MulticastPort As Integer = 42069 'Multicast port for UDP, matches controller's port
    Dim Multicaster As New System.Net.Sockets.UdpClient(MulticastPort) 'Call out the UDP for use
    Dim outputtestindex As Integer = 0  'For testing the outputs of the controller
    Public outputtest As Boolean = False    'Output test is on / off
    Dim outputtestdelay As Integer = 0  'Delay for each output during test.1
    Public Settingschanged As Boolean = False   'A setting has been changed, this'll prompt the user for saving
    Dim VRCopen As Boolean = False  'VRChat is open

#Region "Settings File"
    'Version number (int)
    'Count of Devices (int)
    'Device Name 1 (string)
    'Device Outputs 1 (int)
    'Node Names (string) * device outputs
    'Node Forces (int) *device outputs
#End Region

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load    'This event for the form load

        'Dim p() As Process = Process.GetProcesses() 'Even though this program doesn't interface directly with VRChat, if the debug reader opens before VRC, then VRC may not create a debug file.
        'For i = 0 To p.Length - 1   'Every process happening...
        '    If p(i).ProcessName = "VRChat" Then 'If one of them is VRChat...
        '        VRCopen = True  'Set VRCopen
        '    End If
        'Next
        Try
            If My.Settings.DefFolder = "" Then
                AppDatafolder = GetFolderPath(SpecialFolder.LocalApplicationData) & "Low\VRChat\VRChat\OSC\"
            Else
                AppDatafolder = My.Settings.DefFolder
            End If
            ReadinJSONs()
        Catch ex As Exception
            MsgBox("Error in finding avatar definition files, please define folder under 'File'")
        End Try




        RichTextBox1.Text = "" 'clear history

        'OSC Reciever 
        OSCR.Client.ReceiveTimeout = 10 'How long to wait before moving on
        OSCR.Client.Blocking = False 'Don't allow it to block processing
        OSCep = New System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 0) 'Defining endpoint, used in OSC Comms
        MainTimer.Interval = 50 'Main timer should be about 50ms, so not to overload network i guess

        'Multicaster.JoinMulticastGroup(System.Net.IPAddress.Parse("239.80.8.5"), 10)    'Set multicast IP address, matches on controller
        Multicaster.Client.Blocking = False 'Dont allow it to block processing
        Multicaster.Client.ReceiveTimeout = 100 'Timeout
        Multicaster.MulticastLoopback = False
        Multicaster.DontFragment = True
        Multicaster.EnableBroadcast = True

        'Try to load last settings
        If IO.File.Exists(My.Settings.SettingsLoc) = True Then
            OFDSettings.FileName = My.Settings.SettingsLoc  'Grab settings file from program memory location
            SettingsFileLoad() 'Load it!
        End If

        OSCTimer.Enabled = True 'Enable the OSC read in timer
        MainTimer.Enabled = True 'Enable that main timer

    End Sub

    'UDP Receive
    Public Sub udprcv() 'Called from main timer, reads from controllers to see if they're online
        Dim ep As System.Net.IPEndPoint = New System.Net.IPEndPoint(System.Net.IPAddress.Parse("239.80.8.5"), MulticastPort)    'Setup end point, its for the UDP multicast

        Try
            Dim rcvbytes() As Byte = Multicaster.Receive(ep)    'Define the bytes, retrieve anything out there
            Dim UDPString As String = System.Text.Encoding.ASCII.GetString(rcvbytes)    'Make those bytes into characters
            For i = 0 To NodeDeviceNames.Count - 1  'For each controller we have defined in our program...
                If UDPString.Length >= NodeDeviceNames(i).Length And UDPString.Contains("^P&") = False Then    'Does the recieved UDP packet match the name in length at least?
                    If UDPString.Substring(0, NodeDeviceNames(i).Length) = NodeDeviceNames(i) Then 'If yes, does the name itself match?
                        DGVDevice.Rows(i).Cells(0).Style.BackColor = Color.Green    'Yes found it color the box
                        DGVDevice.Rows(i).Cells(1).Style.BackColor = Color.Green
                        NodeDeviceConnection(i) = 0 'Reset timeout counter
                    End If
                End If
            Next
        Catch ex As Exception
        End Try

    End Sub

    'Main Timer
    Private Sub MainTimer_Tick(sender As Object, e As EventArgs) Handles MainTimer.Tick 'fast tick, like 10ms
        For i = 0 To NodeDeviceNames.Count + 1 'For each controller we have, hit the UDP that many times... plus 1, so we get all names out there
            udprcv()    'Call UDP recieve
        Next

        For i = 0 To NodeDeviceNames.Count - 1 'for each device we have defined
            NodeDeviceConnection(i) = NodeDeviceConnection(i) + 1   'Add 1 to a timer, which is reset if we get a name packet from the device
            If NodeDeviceConnection(i) > 300 Then   'If its gotten past 3 seconds of no name..
                DGVDevice.Rows(i).Cells(0).Style.BackColor = Color.White 'Then mark it as default white
                DGVDevice.Rows(i).Cells(1).Style.BackColor = Color.White
            End If
        Next

        'Outputs!
        For i = 0 To NodeOutputs.Count - 1 'For each device
            For i2 = 0 To NodeOutputs(i).Count - 1 'For each node on the device
                If OSCPSaved = NodeNames(i)(i2) And OSCValueSaved = "True" Then
                    DGVNodes.Rows(i2).Cells(0).Style.BackColor = Color.Green
                    NodeOutputs(i)(i2) = True
                End If
                If OSCPSaved = NodeNames(i)(i2) And OSCValueSaved = "False" Then
                    DGVNodes.Rows(i2).Cells(0).Style.BackColor = Color.White
                    NodeOutputs(i)(i2) = False  ' output off
                End If
            Next
        Next

        If outputtest = True Then   'If output test is on
            testednodeD = DGVDevice.SelectedCells(0).RowIndex
            testednodeN = outputtestindex
            NodeOutputs(testednodeD)(testednodeN) = True
            outputtestdelay = outputtestdelay + 1    'Add time for each output
            If outputtestdelay = 20 Then
                outputtestdelay = 0
                outputtestindex = 0
                outputtest = False
                NodeOutputs(testednodeD)(testednodeN) = False
            End If
        Else
            outputtestindex = 0 'If not output testing, set this to zero
        End If

        'Send to Haptic Devices
        If NodeDeviceNames.Count <> 0 Then 'We got devices?
            For i = 0 To NodeDeviceNames.Count - 1 'For each device we've got defined
                Dim ep As System.Net.IPEndPoint = New System.Net.IPEndPoint(System.Net.IPAddress.Parse("239.80.8.5"), MulticastPort)    'End point stuff for the UDP multicast
                Dim stringz As String = NodeDeviceNames(i) & "^P&" 'Carret is for limiting Name from type of mode and ampersand is for transistion to outputs
                Dim Prefix() As Byte = System.Text.Encoding.ASCII.GetBytes(stringz) 'Convert from chars to bytes (programmers should realize this is silly) Wait bytes are characters? Always has been... gunshot
                Dim sendbytes(Prefix.Length + 19) As Byte   'Define byte length
                For i2 = 0 To Prefix.Length - 1
                    sendbytes(i2) = Prefix(i2) 'Shift in the characters
                Next
                For i2 = 0 To NodeOutputs(i).Count - 1
                    If NodeOutputs(i)(i2) = True Then 'We outputing this output outputedly?
                        sendbytes(Prefix.Length + i2) = NodeForce(i)(i2) * 2.55 'Shift in output data for the device to use
                    Else
                        sendbytes(Prefix.Length + i2) = 0   'Else turn it off
                    End If
                Next
                Multicaster.Send(sendbytes, sendbytes.Length, ep)   'SEND TO THE DEVICE!
            Next
        End If

        GC.Collect() 'House Keeping
    End Sub

    'Delete Device
    Private Sub RemoveDeviceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RemoveDeviceToolStripMenuItem.Click
        If DGVDevice.SelectedCells.Count > 0 Then
            If DGVDevice.SelectedCells(0).RowIndex <> -1 Then 'For the specific listed device
                If MessageBox.Show("Delete " & NodeDeviceNames(DGVDevice.SelectedCells(0).RowIndex), "Really?", MessageBoxButtons.YesNo) = DialogResult.Yes Then 'Confirm deletion
                    NodeDeviceNames.RemoveAt(DGVDevice.SelectedCells(0).RowIndex) 'Remove device from lists
                    NodeDeviceConnection.RemoveAt(DGVDevice.SelectedCells(0).RowIndex)
                    NodeOutputs.RemoveAt(DGVDevice.SelectedCells(0).RowIndex)
                    NodeNames.RemoveAt(DGVDevice.SelectedCells(0).RowIndex)
                    NodeForce.RemoveAt(DGVDevice.SelectedCells(0).RowIndex)
                    DGVDevice.Rows.RemoveAt(DGVDevice.SelectedCells(0).RowIndex)
                    DGVNodeUpdate() 'update nodes
                End If
            End If
        End If
    End Sub

    'Edit Device
    Private Sub EditDeviceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EditDeviceToolStripMenuItem.Click
        If DGVDevice.SelectedCells.Count > 0 Then
            If DGVDevice.SelectedCells(0).RowIndex <> -1 Then
                DeviceMethod = 2 'Editing device
                DeviceIndex = DGVDevice.SelectedCells(0).RowIndex
                DeviceEditor.ShowDialog()
            End If
        End If
    End Sub

    'Add Device
    Private Sub AddDeviceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AddDeviceToolStripMenuItem.Click
        DeviceMethod = 1 'Adding device
        DeviceEditor.ShowDialog()
    End Sub

    'Update DGVNode call from choosing new device
    Private Sub DGVDevice_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DGVDevice.CellClick
        DGVNodeUpdate()
    End Sub
    Public Sub DGVNodeUpdate()
        DGVupdating = True  'Ignore data updating
        DGVNodes.Rows.Clear()   'Empty out Data grid view
        If NodeDeviceNames.Count = 0 Then   'If there's no devices, don't update
            Return
        End If
        For i = 0 To NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count - 1 'After nodes cleared, reload
            DGVNodes.Rows.Add()
            DGVNodes.Rows(DGVNodes.Rows.Count - 1).Cells(0).Value = i
            DGVNodes.Rows(DGVNodes.Rows.Count - 1).Cells(1).Value = NodeNames(DGVDevice.SelectedCells(0).RowIndex)(i)
            DGVNodes.Rows(DGVNodes.Rows.Count - 1).Cells(2).Value = NodeForce(DGVDevice.SelectedCells(0).RowIndex)(i)
        Next
        DGVupdating = False 'This prevents the cell value changed event from happening cause its all original data
    End Sub

    'Save DGV
    Private Sub DGVNodes_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles DGVNodes.CellValueChanged
        If DGVupdating = False Then 'Check we're not just getting the original data
            If e.RowIndex <> -1 Then
                NodeNames(DGVDevice.SelectedCells(0).RowIndex)(e.RowIndex) = DGVNodes.Rows(e.RowIndex).Cells(1).Value 'If Name changed, save that
                Try
                    NodeForce(DGVDevice.SelectedCells(0).RowIndex)(e.RowIndex) = DGVNodes.Rows(e.RowIndex).Cells(2).Value
                    DGVNodes.Rows(e.RowIndex).Cells(2).Style.BackColor = Color.White
                    If DGVNodes.Rows(e.RowIndex).Cells(2).Value < 0 Then
                        DGVNodes.Rows(e.RowIndex).Cells(2).Value = 0
                    ElseIf DGVNodes.Rows(e.RowIndex).Cells(2).Value > 100 Then
                        DGVNodes.Rows(e.RowIndex).Cells(2).Value = 100
                    End If
                Catch ex As Exception
                    DGVNodes.Rows(e.RowIndex).Cells(2).Style.BackColor = Color.Red
                End Try
                Settingschanged = True 'Something changed, prompt at exit for save
            End If
        End If
    End Sub

    'Test button pushed
    Private Sub DGVNodes_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DGVNodes.CellContentClick
        Dim senderGrid = DirectCast(sender, DataGridView)
        If TypeOf senderGrid.Columns(e.ColumnIndex) Is DataGridViewButtonColumn AndAlso
            e.RowIndex >= 0 Then
            outputtest = True
            outputtestindex = e.RowIndex
        End If
    End Sub

    'Devices update
    Public Sub DGVDevicesUpdate() 'Clears out the devices and reloads them
        DGVDevice.Rows.Clear()
        For i = 0 To NodeDeviceNames.Count - 1
            DGVDevice.Rows.Add()
            DGVDevice.Rows(DGVDevice.Rows.Count - 1).Cells(0).Value = i
            DGVDevice.Rows(DGVDevice.Rows.Count - 1).Cells(1).Value = NodeDeviceNames(i)
        Next

    End Sub

    'Settings File
    Private Sub LoadDeviceNodeDescriptionToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LoadDeviceNodeDescriptionToolStripMenuItem.Click
        'Check to see if we already have settings and prompt
        'If NodeDeviceNames.Count <> 0 Then
        'If MessageBox.Show("Overwrite current settings?", "But wait", MessageBoxButtons.OKCancel) = MessageBoxButtons.OK Then
        'OFDSettings.ShowDialog()    'Load the Dialog
        'End If
        'Else
        OFDSettings.ShowDialog()    'Load the Dialog
        'End If

    End Sub
    Private Sub SaveDeviceNodeDescriptionToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveDeviceNodeDescriptionToolStripMenuItem.Click
        'Button to chose settings file to save
        SFDSettings.ShowDialog()
    End Sub
    Private Sub OFDSettings_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OFDSettings.FileOk
        SettingsFileLoad() 'Call File loader
    End Sub
    Private Sub SFDSettings_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles SFDSettings.FileOk
        SettingsFileSave() 'Call File Saver
    End Sub

    'Load settings file
    Private Sub SettingsFileLoad()
        'Empty out everything
        NodeDeviceNames.Clear()
        NodeOutputs.Clear()
        NodeDeviceConnection.Clear()
        NodeNames.Clear()
        NodeForce.Clear()

        DGVDevice.Rows.Clear()

        Dim readstring() As String  'Define the string to read into

        GC.Collect()    'Garbage collection probably correct right?
        readstring = System.IO.File.ReadAllLines(OFDSettings.FileName) 'Read the file into the string array!
        GC.Collect()    'I mean, its probably the worst thing I could do here

        If readstring(0) >= 2 Then   'Version control! Other versions can have elseif >=2 or something
            'intensity = readstring(1)
            Dim DeviceCount As Integer = readstring(1)
            Dim vectorsplit() As String 'Split the vector to 3 parts
            Dim linesread As Integer = 2    'Math is hard
            For i = 0 To DeviceCount - 1
                NodeDeviceNames.Add(readstring(linesread))
                NodeOutputs.Add(New List(Of Boolean))
                For i2 = 0 To Int(readstring(1 + linesread)) - 1
                    NodeOutputs(NodeOutputs.Count - 1).Add(False)
                Next
                'NodeDeviceConnection.Add(readstring(2 + linesread))
                NodeDeviceConnection.Add(0)
                linesread = linesread + 2
                NodeNames.Add(New List(Of String))
                NodeForce.Add(New List(Of Integer))
                For i2 = 0 To NodeOutputs(i).Count - 1
                    NodeNames(NodeDeviceNames.Count - 1).Add(readstring(linesread))
                    linesread = linesread + 1
                Next
                For i2 = 0 To NodeOutputs(i).Count - 1
                    NodeForce(NodeDeviceNames.Count - 1).Add(readstring(linesread))
                    linesread = linesread + 1
                Next

                'TrackBar1.Value = intensity
                DGVDevice.Rows.Add()
                DGVDevice.Rows(DGVDevice.Rows.Count - 1).Cells(0).Value = NodeDeviceNames.Count
                DGVDevice.Rows(DGVDevice.Rows.Count - 1).Cells(1).Value = NodeDeviceNames(NodeDeviceNames.Count - 1)
                DGVNodeUpdate()

            Next

        End If

        My.Settings.SettingsLoc = OFDSettings.FileName  'Drop this as the last saved file so it reopens on load
        SFDSettings.FileName = OFDSettings.FileName
        Settingschanged = False

    End Sub

    'Settings file save
    Private Sub SettingsFileSave()
        Dim WriteList As New List(Of String)

        WriteList.Add("2")  'Version number
        'WriteList.Add(intensity)
        WriteList.Add(NodeDeviceNames.Count.ToString)
        For i = 0 To NodeDeviceNames.Count - 1
            WriteList.Add(NodeDeviceNames(i))
            WriteList.Add(NodeOutputs(i).Count)
            For i2 = 0 To NodeNames(i).Count - 1
                WriteList.Add(NodeNames(i)(i2).ToString)
            Next
            For i2 = 0 To NodeForce(i).Count - 1
                WriteList.Add(NodeForce(i)(i2))
            Next
        Next

        Dim writestring() As String = WriteList.ToArray 'Convert List To array for file writer

        GC.Collect()    'Some Garbage collection cause I got superstition that this helps
        System.IO.File.WriteAllLines(SFDSettings.FileName, writestring) 'Write that file from the array.
        GC.Collect()    'More Garbage collection cause it can't hurt right?

        My.Settings.SettingsLoc = SFDSettings.FileName  'Drop this as the last saved file so it reopens on load
        Settingschanged = False
    End Sub

    'Close program
    Private Sub CloseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CloseToolStripMenuItem.Click
        If MessageBox.Show("Save Settings?", "Save", MessageBoxButtons.YesNo) = DialogResult.Yes Then   'Prompt for file save
            SFDSettings.ShowDialog()    'If yes
        End If
        Me.Close()  'Exit Program
    End Sub

    'Setup device on wifi show dialog
    Private Sub SetWifiOnDeviceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SetWifiOnDeviceToolStripMenuItem.Click
        DeviceWifi.Show()
    End Sub

    'Form closing, request save if something changed
    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If Settingschanged = True Then
            If MessageBox.Show("Save Settings?", "Save", MessageBoxButtons.YesNo) = DialogResult.Yes Then   'Prompt for file save
                SFDSettings.ShowDialog()    'If yes
            End If
        End If
    End Sub

    Private Sub OSCTimer_Tick(sender As Object, e As EventArgs) Handles OSCTimer.Tick
        Try
            Dim OSCbytes() As Byte = OSCR.Receive(OSCep) 'Bytes from OSC
            Dim value(20) As Byte 'Going to store OSC recieved value, in bytes
            Dim param(100) As Byte 'Going to store OSC recieved parameter name
            Dim valuei As Integer = 0 'Indexing for pulling the value out
            Dim parami As Integer = 0 'Indexing for pulling the param name out
            For i = 0 To OSCbytes.Length - 1 'For all the bytes...
                If OSCbytes(i) = 44 Or valuei > 0 Then 'look for the first comma (ASCII 44)
                    value(valuei) = OSCbytes(i) 'once you found it, save the value into a new byte set
                    valuei = valuei + 1 'index it up
                Else
                    param(parami) = OSCbytes(i) 'If you haven't found it yet, save the bytes for parameter name
                    parami = parami + 1 'index that up
                End If
            Next

            'Param
            OSCP = System.Text.Encoding.ASCII.GetString(param) 'Convert the parameter name bytes to a string
            OSCP = OSCP.Trim(vbNullChar) ' Get rid of any extra null characters

            'Value
            'comma then type, 102 = f for float, 105 = i for int
            If value(1) = 102 Then 'If this is a floating point value (-1.0 to 1.0)
                Dim floatvalue(3) As Byte 'Seperate those bytes
                For i = 0 To 3
                    floatvalue(3 - i) = value(4 + i) 'Reverse order them cause why not?
                Next
                Dim valuefloat As Single = BitConverter.ToSingle(floatvalue, 0) 'Use fancy windows magic to convert it lol
                OSCvalue = valuefloat.ToString 'Save it to a string for displaying
            End If
            If value(1) = 105 Then 'If its a integer value
                Dim valueint As Integer = value(7) 'Pull that int value out
                OSCvalue = valueint.ToString 'Save it to string
            End If
            If value(1) = 84 Then 'If its a boolean, and an ASCII 'T' for true then save it as a string for display
                OSCvalue = "True"
            End If
            If value(1) = 70 Then 'If its a boolean, and an ASCII 'F' for false then save it as a string for display
                OSCvalue = "False"
            End If

            'Remove any VRChat default values that get sent
            Dim Defaultparam As Boolean = False
            If OSCP.Contains("IsLocal") Then
                Defaultparam = True
            ElseIf OSCP.Contains("Viseme") Then
                Defaultparam = True
            ElseIf OSCP.Contains("Voice") Then
                Defaultparam = True
            ElseIf OSCP.Contains("GestureLeft") Then
                Defaultparam = True
            ElseIf OSCP.Contains("GestureRight") Then
                Defaultparam = True
            ElseIf OSCP.Contains("GestureLeftWeight") Then
                Defaultparam = True
            ElseIf OSCP.Contains("GestureRightWeight") Then
                Defaultparam = True
            ElseIf OSCP.Contains("AngularY") Then
                Defaultparam = True
            ElseIf OSCP.Contains("VelocityX") Then
                Defaultparam = True
            ElseIf OSCP.Contains("VelocityY") Then
                Defaultparam = True
            ElseIf OSCP.Contains("VelocityZ") Then
                Defaultparam = True
            ElseIf OSCP.Contains("Upright") Then
                Defaultparam = True
            ElseIf OSCP.Contains("Grounded") Then
                Defaultparam = True
            ElseIf OSCP.Contains("Seated") Then
                Defaultparam = True
            ElseIf OSCP.Contains("AFK") Then
                Defaultparam = True
            ElseIf OSCP.Contains("TrackingType") Then
                Defaultparam = True
            ElseIf OSCP.Contains("VRMode") Then
                Defaultparam = True
            ElseIf OSCP.Contains("MuteSelf") Then
                Defaultparam = True
            ElseIf OSCP.Contains("InStation") Then
                Defaultparam = True
            ElseIf OSCP.Contains("Expression") Then
                Defaultparam = True
            End If

            OSCP = OSCP.Substring(OSCP.LastIndexOf("/") + 1)

            If Defaultparam = False Then
                OSCPSaved = OSCP
                OSCValueSaved = OSCvalue
                RichTextBox1.AppendText(OSCP & " - " & OSCvalue) 'Display any unique params
                RichTextBox1.AppendText(Chr(13)) 'Slap a new line
                RichTextBox1.ScrollToCaret()
            End If

        Catch ex As Exception
            'RichTextBox1.AppendText("Data wait")
            'RichTextBox1.AppendText(Chr(13)) 'Slap a new line
            'RichTextBox1.ScrollToCaret()
        End Try
    End Sub

    Private Sub SelectOSCFolderToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SelectOSCFolderToolStripMenuItem.Click
        MsgBox("Looking for the folder: C:\Users\<Your name>\AppData\LocalLow\VRChat\VRChat\OSC")
        FolderBrowserDialog1.SelectedPath = AppDatafolder
        FolderBrowserDialog1.ShowDialog()
    End Sub
    Private Sub FolderBrowserDialog1_HelpRequest(sender As Object, e As EventArgs) Handles FolderBrowserDialog1.HelpRequest
        AppDatafolder = FolderBrowserDialog1.SelectedPath
        ReadinJSONs()
    End Sub

    Public Sub ReadinJSONs()
        Try
            Dim vrcusers() As String = System.IO.Directory.GetDirectories(AppDatafolder)
            For i = 0 To vrcusers.Count - 1
                Dim vrcuseravdir() As String = System.IO.Directory.GetDirectories(vrcusers(i))
                For i2 = 0 To vrcuseravdir.Count - 1
                    Dim vrcuseravs() As String = System.IO.Directory.GetFiles(vrcuseravdir(i2))
                    For i3 = 0 To vrcuseravs.Count - 1
                        avatarfiles.Add(vrcuseravs(i3))
                    Next
                Next
            Next
            My.Settings.DefFolder = AppDatafolder
        Catch ex As Exception
            MsgBox("Error reading JSON files")
        End Try

        Dim avatarfileindex As Integer = 0
        Try
            For i = 0 To avatarfiles.Count - 1
                avatarfileindex = i

                Dim readstring() As String  'Define the string to read into
                Dim WriteList As New List(Of String) 'Define file to write back to

                GC.Collect()    'Garbage collection probably correct right?
                readstring = System.IO.File.ReadAllLines(avatarfiles(i)) 'Read the file into the string array!
                GC.Collect()    'I mean, its probably the worst thing I could do here
                For i2 = 0 To readstring.Count - 1

                    If readstring(i2).Contains("{") Then
                        If readstring(i2 + 1).Contains("""name"":") Then

                            'Remove any VRChat default values that get sent
                            Dim Defaultparam As Boolean = False
                            If readstring(i2 + 1).Contains("IsLocal") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("Viseme") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("Voice") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("GestureLeft") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("GestureRight") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("GestureLeftWeight") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("GestureRightWeight") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("AngularY") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("VelocityX") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("VelocityY") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("VelocityZ") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("Upright") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("Grounded") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("Seated") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("AFK") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("TrackingType") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("VRMode") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("MuteSelf") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("InStation") Then
                                Defaultparam = True
                            ElseIf readstring(i2 + 1).Contains("Expression") Then
                                Defaultparam = True
                            End If

                            If Defaultparam = False Then
                                WriteList.Add(readstring(i2))
                            End If

                            If Defaultparam = True Then
                                i2 = i2 + 6
                            End If

                        Else
                            WriteList.Add(readstring(i2))
                        End If
                    Else
                        WriteList.Add(readstring(i2))
                    End If
                Next

                'Write back file
                'But First, fix a JSON file thing to remove a comma
                WriteList(WriteList.Count - 3) = "    }"

                Dim writestring() As String = WriteList.ToArray 'Convert List To array for file writer

                GC.Collect()    'Some Garbage collection cause I got superstition that this helps
                System.IO.File.WriteAllLines(avatarfiles(i), writestring) 'Write that file from the array.
                GC.Collect()    'More Garbage collection cause it can't hurt right?


            Next

        Catch ex As Exception
            MsgBox("Error when modifying file: " & avatarfiles(avatarfileindex))
        End Try


    End Sub

End Class
