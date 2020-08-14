'To Do:
'Fix 3D avatar so it doesn't suck
'VRC assisted node placement
'Whitelist
'Different Avatar Offsets
'

Imports OpenTK  '3D display imports
Imports OpenTK.Graphics
Imports OpenTK.Graphics.OpenGL
Imports System.ComponentModel 'Other misc imports
Imports System.Environment
Imports System.Diagnostics

Public Class Form1
    Dim GLLoaded As Boolean = False 'wait til 3D is loaded before doing anything to it
    Dim filez As String = "" 'Files in directory
    Dim dir As String = "Low\VRChat\vrchat\" 'temp fix later
    Dim AppDatafolder As String = GetFolderPath(SpecialFolder.LocalApplicationData) 'Get the local data directory
    Dim logfound As Boolean = False 'Proper Log file found
    Dim reader As IO.StreamReader 'reader to read the file
    Dim Readz As String = "" 'the current line from the log, could or could not be a haptic event
    Dim Hapticsline As Boolean = False 'In case we have multiple lines come in at once, try a few out first
    Dim debugcounter As Integer = 0 'Counter for debug to make sure udon didn't break
    Dim head, hips, chest, RShoulder, LShoulder, RLA, LLA, RHand, LHand, RUL, LUL, RLL, LLL, RFoot, LFoot As Vector3 'Local Player
    Dim headR, RHandR, LHandR, ChestR, HipsR As Quaternion    'Rotations from VRC
    Dim RshoulderR, LShoulderR, RLAR, LLAR, RULR, LULR, RLLR, LLLR As Vector3 ' math'd rotations from bone positions
    Dim OPRTD, OPRID, OPLTD, OPLID, OPHead, OPHips, OPRFoot, OPLFoot As Vector3 'Other Player from debug
    Dim HeadRM, RHandRM, LHandRM, ChestRM, HipsRM, RShoulderM, RLAM, LShoulderM, LLAM, RULM, RLLM, LULM, LLLM As Matrix4 'Rotation matrixes
    Dim HeadVector, RHandV1, RHandV2, LHandV1, LHandV2 As Vector3 'Indicators on the 3D
    Dim CurrentRotation As Matrix4 = New Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
    Dim Enable3D As Boolean = True 'Enable or disable active 3D updates
    Dim OtherPlayer As String   'Name of other player as debug gives it
    Dim OtherPlayers As New List(Of String) 'List of other player names
    Dim OPRTDs As New List(Of Vector3)  'Lists of other player components
    Dim OPRIDs As New List(Of Vector3)
    Dim OPLTDs As New List(Of Vector3)
    Dim OPLIDs As New List(Of Vector3)
    Dim OPHeads As New List(Of Vector3)
    Dim OPHipss As New List(Of Vector3)
    Dim OPRFoots As New List(Of Vector3)
    Dim OPLFoots As New List(Of Vector3)
    Dim OPTime As New List(Of Integer) 'Duration of time since we've seen that player in milliseconds
    Dim lower As Single = 0 'bottom point of the avatar

    Dim mousecontrol As Boolean = False 'Mouse being used on GLcontrol
    Dim mousestartrotate As Point   'Used for rotation math
    Dim mousestartposition As Point 'Used for camera movement
    Dim mousecontroltype As Integer 'Button Pressed
    Dim rotateX, rotateY, positionX, positionY As Single    'Used for rotation math
    Dim zpos As Single = 2  'Zoom position
    Dim ypos As Single = -0.2 'Height of camera
    Dim slowrotate As Boolean = True    'Slow spin of the model
    Dim slowrotateY As Single = 0   'Slow spin rotation
    Public NodeDeviceNames As New List(Of String)    'Name of Device for reference like 'head' or 'right arm' kinda thing
    Public NodeOutputs As New List(Of List(Of Boolean)) 'Output of device
    Public NodeDeviceConnection As New List(Of Integer) 'Connection IP or type
    Public NodeRootBone As New List(Of List(Of Integer))   'Bone Root Node is attached to (see debug packet)
    Public NodeBoneOffset As New List(Of List(Of Vector3))   'Offset from bone axis
    Public NodeFinalPos As New List(Of List(Of Vector3)) 'Final math'd position of the node
    Public NodeActivationDistance As New List(Of List(Of Single)) 'Distance anyone else needs to be to activate the node
    Public DeviceIndex As Integer   'for sharing to the other dialog for device editing
    Public DeviceMethod As Integer = 0  'for sharing to the other dialog for device editing
    Dim DGVupdating As Boolean = False  'DGVNode ignores updates if DGVDevice is changing it
    Dim RootBones As List(Of String) = New List(Of String) From {"Unassigned", "Head", "Hips", "Chest", "Right Upper Arm", "Left Upper Arm", "Right Lower Arm", "Left Lower Arm", "Right Hand", "Left Hand", "Right Upper Leg", "Left Upper Leg", "Right Lower Leg", "Left Lower Leg"}  'Names of root bones

    Dim Avatarlocation As String    'Avatar STL file location
    Dim Avatarpoints As New List(Of Vector3)    'Avatar mesh face points
    Dim Avatarcolors As Drawing.Color   'Color of avatar
    Dim AvatarLoaded As Boolean = False 'Is the avatar loaded? bit
    Dim Avatarnormals As New List(Of Vector3)   'Normal (face direction) of the mesh faces of the avatar
    Dim AvatarRotation As Vector3   'Rotation of the avatar

    Dim MulticastPort As Integer = 2002 'Multicast port for UDP, matches controller's port
    Dim Multicaster As New System.Net.Sockets.UdpClient(MulticastPort) 'Call out the UDP for use
    Dim outputtestindex As Integer = 0  'For testing the outputs of the controller
    Public outputtest As Boolean = False    'Output test is on / off
    Dim intensity As Integer = 0    'Intensity slider
    Public Settingschanged As Boolean = False   'A setting has been changed, this'll prompt the user for saving
    Dim VRCopen As Boolean = False  'VRChat is open

#Region "Debug Packet"
    'Packet for 1 player looks like "Haptics:V1(Bone data)(rotation data)"
    'Packet for 2+ players look like "Haptics:V1(Bone data)(rotation data)^^/^(Other player bone data)Otherplayername^/^(Other player bone data)Otherplayername"

    'Example

    '"Just Me - Haptics:V1(29.3, 45.8, -96.6)(29.7, 42.6, -95.3)(29.7, 43.2, -95.4)(29.9, 44.5, -96.7)(28.6, 44.6, -96.1)(31.3, 42.5, -96.7)(27.2, 42.6, -95.5)(32.1, 40.5, -95.4)(27.7, 40.6, -94.0)(30.8, 40.3, -96.0)(28.4, 40.3, -95.2)(31.0, 35.9, -94.4)(29.1, 35.7, -94.4)(31.5, 31.6, -95.4)(27.9, 31.6, -95.7)(0.2405493,0.06394769,0.01590426,-0.9683977)(0.7164925,0.2387804,0.6534851,0.05079648)(0.3226297,-0.2587059,-0.9103048,0.01809094)(0.2255801,-0.215687,-0.023443,-0.9497596)(0.9821816,0.003167251,-0.1876046,0.01067693)"
    '"One more - Haptics:V1(80.3, 37.7, -334.1)(80.1, 34.3, -334.6)(80.1, 34.9, -334.6)(80.5, 36.6, -335.1)(79.2, 36.6, -334.4)(81.5, 34.7, -336.4)(77.6, 34.7, -334.5)(82.0, 32.3, -336.4)(77.7, 32.3, -334.2)(81.1, 32.0, -335.5)(78.8, 32.0, -334.2)(81.0, 27.7, -333.6)(80.3, 27.8, -332.9)(81.3, 23.5, -335.2)(79.0, 23.6, -333.8)(0.2701674,0.239546,-0.06961434,0.9299361)(-0.7548343,-0.001274519,-0.6484137,0.09891184)(-0.2426835,0.04793017,0.959358,0.135793)(0.04194229,0.2654545,0.006020525,0.963192)(-0.9635401,-0.003260065,0.2675398,0.001444333)^^/^(81.4, 29.9, -324.1)(81.4, 29.7, -324.4)(83.0, 29.9, -322.5)(83.1, 29.7, -322.7)(80.5, 32.7, -321.3)(80.2, 28.9, -321.3)(79.3, 22.3, -322.2)(81.3, 22.4, -321.3)Moogle1"
    '"Two - Haptics:V1(76.9, 37.6, -323.9)(77.0, 34.2, -323.2)(77.0, 34.9, -323.2)(76.4, 36.6, -322.9)(77.9, 36.5, -323.3)(75.2, 34.7, -321.8)(79.5, 34.7, -322.6)(74.9, 32.2, -321.8)(79.4, 32.3, -322.7)(75.9, 32.0, -322.5)(78.4, 31.9, -323.3)(76.1, 27.8, -324.6)(77.2, 27.8, -325.0)(75.7, 23.6, -323.1)(78.4, 23.6, -323.7)(-0.03748857,0.936684,-0.3316957,-0.1058107)(-0.7507744,0.0998488,0.6527701,-0.01611204)(0.9126387,0.1561787,0.3777094,-0.005902914)(0.005238595,0.9890235,-0.07425836,-0.1276343)(0.1449823,0.004352957,0.9894212,0.002602346)^^/^(66.6, 30.4, -332.5)(66.8, 29.7, -333.0)(67.6, 30.3, -329.0)(68.0, 29.7, -328.7)(65.5, 36.4, -330.2)(66.1, 32.0, -330.4)(65.9, 23.0, -331.8)(67.0, 23.0, -328.8)Snoozeto^/^(80.5, 31.6, -329.4)(80.6, 31.4, -328.8)(78.9, 31.1, -330.5)(78.5, 30.6, -330.3)(81.5, 33.4, -332.4)(81.2, 29.5, -332.0)(81.8, 22.4, -331.6)(80.4, 22.4, -332.0)Moogle1"

    '===Local Player=== (bone root index too)
    '1. Head
    '2. Hips
    '3. Chest
    '4. Right Shoulder
    '5. Left Shoulder
    '6. Right Lower Arm (elbow)
    '7. Left Lower Arm (elbow)
    '8. Right Hand
    '9. Left Hand
    '10 Right Upper Leg
    '11 Left Upper Leg
    '12 Right Lower Leg (knee)
    '13 Left Lower Leg (knee)
    'Right Foot
    'Left Foot
    'Head Rotation
    'Right Hand Rotation
    'Left Hand Rotation
    'Chest Rotation
    'Hips Rotation

    '===Other Player===
    'Right Thumb Distal
    'Right Index Distal
    'Left Thumb Distal
    'Left Index Distal
    'Head
    'Hips
    'Right Foot
    'Left Foot

#End Region

#Region "Settings File"
    'Version number (int)
    'Count of Devices (int)
    'Device Name 1 (string)
    'Device Outputs 1 (int)
    'Device Connection 1 (string)
    'Node Root Bone 1 (int)
    'Node Root Bone n
    'Node Root Bone Offset 1 (vector3)
    'Node Root Bone Offset n
    'Node Activation Distance 1 (Single)
    'Node Activation Distance n
    'Device Name n
    '...
    'Whitelist count
    'Whitelist Name 1
    'WhiteList Name n
#End Region

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load    'This event for the form load

        Dim p() As Process = Process.GetProcesses() 'Even though this program doesn't interface directly with VRChat, if the debug reader opens before VRC, then VRC may not create a debug file.
        For i = 0 To p.Length - 1   'Every process happening...
            If p(i).ProcessName = "VRChat" Then 'If one of them is VRChat...
                VRCopen = True  'Set VRCopen
            End If
        Next

        Multicaster.JoinMulticastGroup(System.Net.IPAddress.Parse("239.80.8.5"))    'Set multicast IP address, matches on controller
        Multicaster.Client.Blocking = False 'Dont allow it to block processing
        Multicaster.Client.ReceiveTimeout = 100 'Timeout

        If VRCopen = True Then  'If VRC is open
            Try
                filez = IO.Directory.GetFiles(AppDatafolder & dir, "*.txt").OrderByDescending(Function(f) New IO.FileInfo(f).LastWriteTime).First() 'Get the latest TXT file
                If filez.Contains("output_log") Then 'output_log is in the default log file name
                    Dim fs As New IO.FileStream(filez, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite) 'Setup to be read from while VRC still writing to it
                    reader = New IO.StreamReader(fs) 'Make it a reader
                    reader.ReadToEnd() 'Jump to end since we don't want to hear about everything til now
                    LblLogStatus.Text = "Log file found, reading..."
                    LblLogStatus.ForeColor = Color.Green
                    MainTimer.Enabled = True    'Set main timer to begin running
                    logfound = True 'Set log file found
                Else
                    LblLogStatus.Text = "Another TXT file was found?" 'Should only be the logs in the directory
                    LblLogStatus.ForeColor = Color.Red
                End If

            Catch ex As Exception   'Something went wrong...
                LblLogStatus.Text = "Either no log yet or wrong directory, Retry every 5 seconds..."
                LblLogStatus.ForeColor = Color.Orange
            End Try
        Else
            LblLogStatus.Text = "Waiting for VRChat to open..."
        End If

        'Enable This so it can see retry if the first attempt failed or see if a new file was created.
        RetryTimer.Enabled = True
        MainTimer.Enabled = True

        'Try to load last settings
        If IO.File.Exists(My.Settings.SettingsLoc) = True Then
            OFDSettings.FileName = My.Settings.SettingsLoc  'Grab settings file from program memory location
            SettingsFileLoad() 'Load it!
        End If


        'Set initial values for model, taken from my avatar =3
        head = New Vector3(-37.1, 49.4, -155.9) / 10
        hips = New Vector3(-37.1, 46.0, -155.9) / 10
        chest = New Vector3(-37.2, 46.6, -155.8) / 10
        RShoulder = New Vector3(-37.7, 48.2, -156.6) / 10
        LShoulder = New Vector3(-37.7, 48.2, -155.1) / 10
        RLA = New Vector3(-38.0, 46.2, -158.0) / 10
        LLA = New Vector3(-38.2, 46.3, -153.6) / 10
        RHand = New Vector3(-37.4, 43.8, -158.2) / 10
        LHand = New Vector3(-37.5, 43.9, -153.5) / 10
        RUL = New Vector3(-37.6, 43.7, -157.1) / 10
        LUL = New Vector3(-37.2, 43.7, -154.5) / 10
        RLL = New Vector3(-36.3, 39.2, -156.5) / 10
        LLL = New Vector3(-35.7, 39.3, -155.0) / 10
        RFoot = New Vector3(-37.8, 35.0, -157.3) / 10
        LFoot = New Vector3(-36.8, 35.1, -153.9) / 10
        headR = New Quaternion(0.09596183, 0.7285335, -0.1043496, 0.6701803)
        RHandR = New Quaternion(-0.9803324, -0.04580933, -0.1728171, 0.08357219)
        LHandR = New Quaternion(0.2495387, 0.1442466, 0.957502, 0.0106489)
        ChestR = New Quaternion(-0.03282366, 0.7132464, 0.03047819, 0.6994808)
        HipsR = New Quaternion(-0.6524559, -0.001264208, 0.757825, 0.00106812)

        'Center Model
        Dim difference As Vector3 = hips 'Grab where the center should be
        hips = hips - difference    'Remove that offset from the rest of the points
        head = head - difference
        chest = chest - difference
        RShoulder = RShoulder - difference
        LShoulder = LShoulder - difference
        RLA = RLA - difference
        LLA = LLA - difference
        RHand = RHand - difference
        LHand = LHand - difference
        RUL = RUL - difference
        LUL = LUL - difference
        RLL = RLL - difference
        LLL = LLL - difference
        RFoot = RFoot - difference
        LFoot = LFoot - difference
        OPRTD = OPRTD - difference
        OPRID = OPRID - difference
        OPLTD = OPLTD - difference
        OPLID = OPLID - difference
        OPHead = OPHead - difference
        OPHips = OPHips - difference
        OPRFoot = OPRFoot - difference
        OPLFoot = OPLFoot - difference

        If RFoot.Y < LFoot.Y Then   'Get lowest point on avatar for 3d orientation
            lower = RFoot.Y
        Else
            lower = LFoot.Y
        End If

    End Sub

    Private Sub RetryTimer_Tick(sender As Object, e As EventArgs) Handles RetryTimer.Tick   'This runs to find / handle VRChat closing and reopening

        'This part is the same as abover in the form load event
        Dim p() As Process = Process.GetProcesses() 'Even though this program doesn't interface directly with VRChat, if the debug reader opens before VRC, then VRC may not create a debug file.
        VRCopen = False
        For i = 0 To p.Length - 1
            If p(i).ProcessName = "VRChat" Then
                VRCopen = True
            End If
        Next
        If VRCopen = True Then

            If logfound = False Then    'If the log file wasn't found the first time
                Try
                    filez = IO.Directory.GetFiles(AppDatafolder & dir, "*.txt").OrderByDescending(Function(f) New IO.FileInfo(f).LastWriteTime).First() 'Get the latest TXT file
                    If filez.Contains("output_log") Then 'output_log is in the default log file name
                        Dim fs As New IO.FileStream(filez, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite) 'Setup to be read from while VRC still writing to it
                        reader = New IO.StreamReader(fs) 'Make it a reader
                        reader.ReadToEnd() 'Jump to end since we don't want to hear about everything til now
                        LblLogStatus.Text = "Log file found, reading..."
                        LblLogStatus.ForeColor = Color.Green
                        MainTimer.Enabled = True
                        logfound = True
                    Else
                        LblLogStatus.Text = "Another TXT file was found?"
                        LblLogStatus.ForeColor = Color.Red
                    End If

                Catch ex As Exception
                    LblLogStatus.Text = "Either no log yet or wrong directory, Retry every 5 seconds..."
                    LblLogStatus.ForeColor = Color.Orange
                End Try

            Else    'If the log file was found and is running, just gotta check if a new one was created. Used for when closing and reopening VRC.

                Try
                    Dim latestfile As String = IO.Directory.GetFiles(AppDatafolder & dir, "*.txt").OrderByDescending(Function(f) New IO.FileInfo(f).LastWriteTime).First() 'Get the latest TXT file
                    If latestfile.Contains("output_log") Then 'output_log is in the default log file name
                        If latestfile <> filez Then
                            Dim fs As New IO.FileStream(latestfile, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite) 'Setup to be read from while VRC still writing to it
                            reader = New IO.StreamReader(fs) 'Make it a reader
                            reader.ReadToEnd() 'Jump to end since we don't want to hear about everything til now
                            LblLogStatus.Text = "New Log file found, reading..."
                            LblLogStatus.ForeColor = Color.Green
                        End If
                    End If

                Catch ex As Exception
                    LblLogStatus.Text = "New file was found but doesn't work."
                    LblLogStatus.ForeColor = Color.Red
                End Try


            End If
        Else
            LblLogStatus.Text = "Waiting for VRChat to open..."
            LblLogStatus.ForeColor = Color.Black
        End If

    End Sub

    Private Sub LoadAvatarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LoadAvatarToolStripMenuItem.Click
        OFDAvatar.ShowDialog()  'Open Avatar File Dialog
    End Sub

    Private Sub OFDAvatar_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OFDAvatar.FileOk
        Avatarpoints.Clear()    'Clear avatar anything in points
        Avatarlocation = OFDAvatar.FileName 'Location of file
        fileimportSTL() 'Import it
        GlControl1.Invalidate() 'Refresh 3D
    End Sub

    'UDP Receive
    Public Sub udprcv() 'Called from main timer, reads from controllers to see if they're online
        Dim ep As System.Net.IPEndPoint = New System.Net.IPEndPoint(System.Net.IPAddress.Parse("239.80.8.5"), MulticastPort)    'Setup end point, its for the UDP multicast

        Try
            Dim rcvbytes() As Byte = Multicaster.Receive(ep)    'Define the bytes, retrieve anything out there
            Dim UDPString As String = System.Text.Encoding.ASCII.GetString(rcvbytes)    'Make those bytes into characters
            For i = 0 To NodeDeviceNames.Count - 1  'For each controller we have defined in our program...
                If UDPString.Length = NodeDeviceNames(i).Length Then    'Does the recieved UDP packet match the name in length at least?
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

        If slowrotate = True Then   'That super sexy slow rotate of the model
            slowrotateY = slowrotateY + 0.5 'Nice and slow, just like daddy likes it
        End If

        If VRCopen = True Then
            Readz = reader.ReadLine() 'get line from file stream
        End If

        Try 'Call this the Corb catcher, catches errors in the packets and just exits.

            If Readz <> "" Then 'Make sure its not a blank line

                If Readz.Contains("Haptics:") Then 'If its part of our haptics stuff...

                    If Readz.Contains("Debug") Then 'Debug Counter, only really used in my test world, nothing to see here
                        debugcounter = debugcounter + 1 'debug counter increase
                    Else

                        'Get local player bones
                        'Head
                        Dim headchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Head from string
                        Dim headpieces As String() = headchunk.Split(",") 'Split it apart
                        head.X = headpieces(0) / 10 'Head pieces to the vector3
                        head.Y = headpieces(1) / 10
                        head.Z = headpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Hips
                        Dim hipschunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just hips from string
                        Dim hipspieces As String() = hipschunk.Split(",") 'Split it apart
                        hips.X = hipspieces(0) / 10 'Hips pieces to the vector3
                        hips.Y = hipspieces(1) / 10
                        hips.Z = hipspieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Chest
                        Dim Chestchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Chest from string
                        Dim Chestpieces As String() = Chestchunk.Split(",") 'Split it apart
                        chest.X = Chestpieces(0) / 10 'Chest pieces to the vector3
                        chest.Y = Chestpieces(1) / 10
                        chest.Z = Chestpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Right Shoulder
                        Dim RShoulderchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Right Shoulder from string
                        Dim RShoulderpieces As String() = RShoulderchunk.Split(",") 'Split it apart
                        RShoulder.X = RShoulderpieces(0) / 10 'Right Shoulder pieces to the vector3
                        RShoulder.Y = RShoulderpieces(1) / 10
                        RShoulder.Z = RShoulderpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Left Shoulder
                        Dim lShoulderchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Left Shoulder from string
                        Dim lShoulderpieces As String() = lShoulderchunk.Split(",") 'Split it apart
                        LShoulder.X = lShoulderpieces(0) / 10 'Left Shoulder pieces to the vector3
                        LShoulder.Y = lShoulderpieces(1) / 10
                        LShoulder.Z = lShoulderpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Right Lower Arm (elbow)
                        Dim RLAchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Right Lower Arm (elbow) from string
                        Dim RLApieces As String() = RLAchunk.Split(",") 'Split it apart
                        RLA.X = RLApieces(0) / 10 'Right Lower Arm (elbow) pieces to the vector3
                        RLA.Y = RLApieces(1) / 10
                        RLA.Z = RLApieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Left Lower Arm (elbow)
                        Dim LLAchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Left Lower Arm (elbow) from string
                        Dim LLApieces As String() = LLAchunk.Split(",") 'Split it apart
                        LLA.X = LLApieces(0) / 10 'Left Lower Arm (elbow) pieces to the vector3
                        LLA.Y = LLApieces(1) / 10
                        LLA.Z = LLApieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Right Hand
                        Dim RHandchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Right Hand from string
                        Dim RHandpieces As String() = RHandchunk.Split(",") 'Split it apart
                        RHand.X = RHandpieces(0) / 10 'Right Hand pieces to the vector3
                        RHand.Y = RHandpieces(1) / 10
                        RHand.Z = RHandpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Left Hand
                        Dim LHandchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Left Hand from string
                        Dim LHandpieces As String() = LHandchunk.Split(",") 'Split it apart
                        LHand.X = LHandpieces(0) / 10 'Left Hand pieces to the vector3
                        LHand.Y = LHandpieces(1) / 10
                        LHand.Z = LHandpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Right Upper Leg
                        Dim Rulchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Right Upper Leg from string
                        Dim Rulpieces As String() = Rulchunk.Split(",") 'Split it apart
                        RUL.X = Rulpieces(0) / 10 'Right Upper Leg pieces to the vector3
                        RUL.Y = Rulpieces(1) / 10
                        RUL.Z = Rulpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Left Upper Leg
                        Dim lulchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Left Upper Leg from string
                        Dim lulpieces As String() = lulchunk.Split(",") 'Split it apart
                        LUL.X = lulpieces(0) / 10 'Left Upper Leg pieces to the vector3
                        LUL.Y = lulpieces(1) / 10
                        LUL.Z = lulpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Right Lower Leg (knee)
                        Dim Rllchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Right Lower Leg (knee) from string
                        Dim Rllpieces As String() = Rllchunk.Split(",") 'Split it apart
                        RLL.X = Rllpieces(0) / 10 'Right Lower Leg (knee) pieces to the vector3
                        RLL.Y = Rllpieces(1) / 10
                        RLL.Z = Rllpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Left Lower Leg (knee)
                        Dim lllchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Left Lower Leg (knee) from string
                        Dim lllpieces As String() = lllchunk.Split(",") 'Split it apart
                        LLL.X = lllpieces(0) / 10 'Left Lower Leg (knee) pieces to the vector3
                        LLL.Y = lllpieces(1) / 10
                        LLL.Z = lllpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Right Foot
                        Dim RFootchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Right Foot from string
                        Dim RFootpieces As String() = RFootchunk.Split(",") 'Split it apart
                        RFoot.X = RFootpieces(0) / 10 'Right Foot pieces to the vector3
                        RFoot.Y = RFootpieces(1) / 10
                        RFoot.Z = RFootpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Left Foot
                        Dim lFootchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Left Foot from string
                        Dim lFootpieces As String() = lFootchunk.Split(",") 'Split it apart
                        LFoot.X = lFootpieces(0) / 10 'Left Foot pieces to the vector3
                        LFoot.Y = lFootpieces(1) / 10
                        LFoot.Z = lFootpieces(2) / 10
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Head Rotation
                        Dim HeadRchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Head Rotation from string
                        Dim HeadRpieces As String() = HeadRchunk.Split(",") 'Split it apart
                        headR.X = HeadRpieces(0) 'Head Rotation pieces to the vector3
                        headR.Y = HeadRpieces(1)
                        headR.Z = HeadRpieces(2)
                        headR.W = HeadRpieces(3)
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Right Hand Rotation
                        Dim RHandRchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Right Hand Rotation from string
                        Dim RHandRpieces As String() = RHandRchunk.Split(",") 'Split it apart
                        RHandR.X = RHandRpieces(0) 'Right Hand Rotation pieces to the vector3
                        RHandR.Y = RHandRpieces(1)
                        RHandR.Z = RHandRpieces(2)
                        RHandR.W = RHandRpieces(3)
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Left Hand Rotation
                        Dim LHandRchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Left Hand Rotation from string
                        Dim LHandRpieces As String() = LHandRchunk.Split(",") 'Split it apart
                        LHandR.X = LHandRpieces(0) 'Left Hand Rotation pieces to the vector3
                        LHandR.Y = LHandRpieces(1)
                        LHandR.Z = LHandRpieces(2)
                        LHandR.W = LHandRpieces(3)
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Chest Rotation
                        Dim ChestRchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Head Rotation from string
                        Dim ChestRpieces As String() = ChestRchunk.Split(",") 'Split it apart
                        ChestR.X = ChestRpieces(0) 'Head Rotation pieces to the vector3
                        ChestR.Y = ChestRpieces(1)
                        ChestR.Z = ChestRpieces(2)
                        ChestR.W = ChestRpieces(3)
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        'Hips Rotation
                        Dim HipsRchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Head Rotation from string
                        Dim HipsRpieces As String() = HipsRchunk.Split(",") 'Split it apart
                        HipsR.X = HipsRpieces(0) 'Head Rotation pieces to the vector3
                        HipsR.Y = HipsRpieces(1)
                        HipsR.Z = HipsRpieces(2)
                        HipsR.W = HipsRpieces(3)
                        Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                        Dim difference As Vector3 = hips 'Grab where the center should be

                        OPRTDs.Clear()  'Clear out other player's bones for this round
                        OPRIDs.Clear()
                        OPLTDs.Clear()
                        OPLIDs.Clear()
                        OPHeads.Clear()
                        OPHipss.Clear()
                        OPRFoots.Clear()
                        OPLFoots.Clear()
                        OtherPlayers.Clear()

                        Dim otherplayercount As Integer = (Readz.Length - Readz.Replace("^/^", String.Empty).Length) / 3 'how man other players are close enough to me?

                        For i = 0 To otherplayercount - 1 'for each of them...

                            'Other Player's Right Thumb Distal
                            Dim OPRTDchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Right Thumb Distal from string
                            Dim OPRTDpieces As String() = OPRTDchunk.Split(",") 'Split it apart
                            OPRTD.X = OPRTDpieces(0) / 10 'Right Thumb Distal pieces to the vector3
                            OPRTD.Y = OPRTDpieces(1) / 10
                            OPRTD.Z = OPRTDpieces(2) / 10
                            Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                            'Other Player's Right Index Distal
                            Dim OPRiDchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Right Index Distal from string
                            Dim OPRiDpieces As String() = OPRiDchunk.Split(",") 'Split it apart
                            OPRID.X = OPRiDpieces(0) / 10 'Right Index Distal pieces to the vector3
                            OPRID.Y = OPRiDpieces(1) / 10
                            OPRID.Z = OPRiDpieces(2) / 10
                            Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                            'Other Player's Left Thumb Distal
                            Dim OPlTDchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Left Thumb Distal from string
                            Dim OPlTDpieces As String() = OPlTDchunk.Split(",") 'Split it apart
                            OPLTD.X = OPlTDpieces(0) / 10 'Left Thumb Distal pieces to the vector3
                            OPLTD.Y = OPlTDpieces(1) / 10
                            OPLTD.Z = OPlTDpieces(2) / 10
                            Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                            'Other Player's Left Index Distal
                            Dim OPliDchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Left Index Distal from string
                            Dim OPliDpieces As String() = OPliDchunk.Split(",") 'Split it apart
                            OPLID.X = OPliDpieces(0) / 10 'Left Index Distal pieces to the vector3
                            OPLID.Y = OPliDpieces(1) / 10
                            OPLID.Z = OPliDpieces(2) / 10
                            Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                            'Other player's Head
                            Dim OPheadchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Head from string
                            Dim OPheadpieces As String() = OPheadchunk.Split(",") 'Split it apart
                            OPHead.X = OPheadpieces(0) / 10 'Head pieces to the vector3
                            OPHead.Y = OPheadpieces(1) / 10
                            OPHead.Z = OPheadpieces(2) / 10
                            Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                            'Other player's Hips
                            Dim OPHipschunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Hips from string
                            Dim OPHipspieces As String() = OPHipschunk.Split(",") 'Split it apart
                            OPHips.X = OPHipspieces(0) / 10 'Hips pieces to the vector3
                            OPHips.Y = OPHipspieces(1) / 10
                            OPHips.Z = OPHipspieces(2) / 10
                            Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                            'Other player's Right Foot
                            Dim OPRFootchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Right Foot from string
                            Dim OPRFootpieces As String() = OPRFootchunk.Split(",") 'Split it apart
                            OPRFoot.X = OPRFootpieces(0) / 10 'Right Foot pieces to the vector3
                            OPRFoot.Y = OPRFootpieces(1) / 10
                            OPRFoot.Z = OPRFootpieces(2) / 10
                            Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                            'Other player's Left Foot
                            Dim OPlFootchunk As String = Readz.Substring(Readz.IndexOf("(") + 1, Readz.IndexOf(")") - Readz.IndexOf("(") - 1) 'Pull out just Left Foot from string
                            Dim OPlFootpieces As String() = OPlFootchunk.Split(",") 'Split it apart
                            OPLFoot.X = OPlFootpieces(0) / 10 'Left Foot pieces to the vector3
                            OPLFoot.Y = OPlFootpieces(1) / 10
                            OPLFoot.Z = OPlFootpieces(2) / 10
                            Readz = Readz.Substring(Readz.IndexOf(")") + 1) 'Split the rest of string for the next set

                            'Get other player Names
                            If i + 1 = otherplayercount Then ' on the last name?
                                OtherPlayers.Add(Readz)
                            Else 'No there's more
                                OtherPlayers.Add(Readz.Substring(0, Readz.IndexOf("^/^"))) 'Like this is so much string manipulation and I can't handle it man
                                Readz = Readz.Substring(Readz.IndexOf("^/^")) 'Take the name off incase they have "(" or ")" in their name
                            End If

                            'Center other player to myself
                            OPRTD = OPRTD - difference
                            OPRID = OPRID - difference
                            OPLTD = OPLTD - difference
                            OPLID = OPLID - difference
                            OPHead = OPHead - difference
                            OPHips = OPHips - difference
                            OPRFoot = OPRFoot - difference
                            OPLFoot = OPLFoot - difference

                            OPRTDs.Add(OPRTD)   'add each other bone of the other players to a list
                            OPRIDs.Add(OPRID)
                            OPLTDs.Add(OPLTD)
                            OPLIDs.Add(OPLID)
                            OPHeads.Add(OPHead)
                            OPHipss.Add(OPHips)
                            OPRFoots.Add(OPRFoot)
                            OPLFoots.Add(OPLFoot)

                        Next

                        'Center Model
                        hips = hips - difference    'Remove that offset from the rest of the points
                        head = head - difference
                        chest = chest - difference
                        RShoulder = RShoulder - difference
                        LShoulder = LShoulder - difference
                        RLA = RLA - difference
                        LLA = LLA - difference
                        RHand = RHand - difference
                        LHand = LHand - difference
                        RUL = RUL - difference
                        LUL = LUL - difference
                        RLL = RLL - difference
                        LLL = LLL - difference
                        RFoot = RFoot - difference
                        LFoot = LFoot - difference

                    End If
                Else
                    reader.ReadToEnd() 'If this wasn't a haptics, then jump to the end. If it was, there is probably more after it!
                End If
            End If

        Catch ex As Exception
        End Try

        'MATH WHAAAAAAAAAAAAT
        HeadRM = Matrix4.Rotate(headR) 'Head rotation math
        RHandRM = Matrix4.Rotate(RHandR) 'Right Hand Rotation Math
        LHandRM = Matrix4.Rotate(LHandR) 'Left Hand Rotation Math
        ChestRM = Matrix4.Rotate(ChestR) 'Chest Rotation Math
        HipsRM = Matrix4.Rotate(HipsR) 'Hips Rotation Math

        If headR = New Quaternion(0, 0, 0, 0) Then  'Quick check to see if the Quat is empty, that way we don't lose our positon
            HeadRM = New Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
        End If
        If RHandR = New Quaternion(0, 0, 0, 0) Then 'Quick check to see if the Quat is empty, that way we don't lose our positon
            RHandRM = New Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
        End If
        If LHandR = New Quaternion(0, 0, 0, 0) Then 'Quick check to see if the Quat is empty, that way we don't lose our positon
            LHandRM = New Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
        End If
        If ChestR = New Quaternion(0, 0, 0, 0) Then 'Quick check to see if the Quat is empty, that way we don't lose our positon
            ChestRM = New Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
        End If
        If HipsR = New Quaternion(0, 0, 0, 0) Then 'Quick check to see if the Quat is empty, that way we don't lose our positon
            HipsRM = New Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
        End If

        RShoulderM = Matrix4.LookAt(RShoulder, RLA, New Vector3(0, 1, 0)) 'Right Shoulder Rotation Math
        RLAM = Matrix4.LookAt(RLA, RHand, New Vector3(0, 1, 0))  'Right Lower Arm Rotation Math
        LShoulderM = Matrix4.LookAt(LShoulder, LLA, New Vector3(0, 1, 0)) 'Left Shoulder Rotation Math
        LLAM = Matrix4.LookAt(LLA, LHand, New Vector3(0, 1, 0)) 'Left Lower Arm Rotation Math
        RULM = Matrix4.LookAt(RUL, RLL, New Vector3(0, 1, 0)) 'Right Upper Leg Rotation Math
        RLLM = Matrix4.LookAt(RLL, RFoot, New Vector3(0, 1, 0)) 'Right Lower Leg Rotation Math
        LULM = Matrix4.LookAt(LUL, LLL, New Vector3(0, 1, 0)) 'Left Upper Leg Rotation Math
        LLLM = Matrix4.LookAt(LLL, LFoot, New Vector3(0, 1, 0)) 'Left Lower Leg Rotation Math


        'Invert all, but put them in -try- because sometimes they fault out
        Try
            RShoulderM.Invert()
            RLAM.Invert()
        Catch ex As Exception
        End Try
        Try
            LShoulderM.Invert()
            LLAM.Invert()
        Catch ex As Exception
        End Try
        Try
            RULM.Invert()
            RLLM.Invert()
        Catch ex As Exception
        End Try
        Try
            LULM.Invert()
            LLLM.Invert()
        Catch ex As Exception
        End Try

        For i = 0 To NodeDeviceNames.Count - 1 'For each device
            For i2 = 0 To NodeBoneOffset(i).Count - 1   'For each node of the device
                NodeFinalPos(i)(i2) = Vector3.Zero  'Make the position zero to start with
                If NodeRootBone(i)(i2) = 1 Then 'If it head
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), HeadRM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + head    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 2 Then 'If it's hips
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), HipsRM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + hips    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 3 Then 'If it's chest
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), ChestRM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + chest    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 4 Then 'If it's right shoulder
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), RShoulderM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + RShoulder    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 5 Then 'If it's left shoulder
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), LShoulderM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + LShoulder    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 6 Then 'If it's right lower arm
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), RLAM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + RLA    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 7 Then 'If it's left lower arm
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), LLAM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + LLA    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 8 Then 'If it's Right hand
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), RHandRM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + RHand    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 9 Then 'If it's Left Hand
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), LHandRM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + LHand    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 10 Then 'If it's Right Upper Leg
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), RULM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + RUL    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 11 Then 'If it's Left Upper Leg
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), LULM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + LUL    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 12 Then 'If it's Right Lower Leg
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), RLLM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + RLL    'Add Offset
                End If
                If NodeRootBone(i)(i2) = 13 Then 'If it's Left Lower Leg
                    NodeFinalPos(i)(i2) = Vector3.TransformVector(NodeBoneOffset(i)(i2), LLLM) 'Apply Rotation
                    NodeFinalPos(i)(i2) = NodeFinalPos(i)(i2) + LLL    'Add Offset
                End If
            Next
        Next

        'Head direction indicator, on the 3D display
        HeadVector = New Vector3(0, 0, V3Distance(head, chest) / 2)
        HeadVector = Vector3.TransformVector(HeadVector, HeadRM)
        HeadVector = HeadVector + head
        'Right Hand indicator
        RHandV1 = New Vector3(V3Distance(head, chest) / 10, V3Distance(head, chest) / 2, 0)
        RHandV2 = New Vector3(-1 * V3Distance(head, chest) / 10, V3Distance(head, chest) / 2, 0)
        RHandV1 = Vector3.TransformVector(RHandV1, RHandRM)
        RHandV2 = Vector3.TransformVector(RHandV2, RHandRM)
        RHandV1 = RHandV1 + RHand
        RHandV2 = RHandV2 + RHand
        'Left Hand indicator
        LHandV1 = New Vector3(V3Distance(head, chest) / 10, V3Distance(head, chest) / 2, 0)
        LHandV2 = New Vector3(-1 * V3Distance(head, chest) / 10, V3Distance(head, chest) / 2, 0)
        LHandV1 = Vector3.TransformVector(LHandV1, LHandRM)
        LHandV2 = Vector3.TransformVector(LHandV2, LHandRM)
        LHandV1 = LHandV1 + LHand
        LHandV2 = LHandV2 + LHand


        'Outputs!
        For i = 0 To NodeFinalPos.Count - 1 'For each device
            For i2 = 0 To NodeFinalPos(i).Count - 1 'For each node on the device
                NodeOutputs(i)(i2) = False  'Start by turning the output off
                For i3 = 0 To OtherPlayers.Count - 1    'For each other player
                    If V3Distance(NodeFinalPos(i)(i2), OPRTDs(i3)) <= NodeActivationDistance(i)(i2) Then 'Is there Right Thumb Distal close?
                        NodeOutputs(i)(i2) = True
                    ElseIf V3Distance(NodeFinalPos(i)(i2), OPLTDs(i3)) <= NodeActivationDistance(i)(i2) Then 'Is there Left Thumb Distal close?
                        NodeOutputs(i)(i2) = True
                    ElseIf V3Distance(NodeFinalPos(i)(i2), OPRIDs(i3)) <= NodeActivationDistance(i)(i2) Then 'Is there Right Index Distal close?
                        NodeOutputs(i)(i2) = True
                    ElseIf V3Distance(NodeFinalPos(i)(i2), OPLIDs(i3)) <= NodeActivationDistance(i)(i2) Then 'Is there Left Index Distal close?
                        NodeOutputs(i)(i2) = True
                    ElseIf V3Distance(NodeFinalPos(i)(i2), OPHeads(i3)) <= NodeActivationDistance(i)(i2) Then 'Is there Head close?
                        NodeOutputs(i)(i2) = True
                    ElseIf V3Distance(NodeFinalPos(i)(i2), OPHipss(i3)) <= NodeActivationDistance(i)(i2) Then 'Is there Hip close?
                        NodeOutputs(i)(i2) = True
                    ElseIf V3Distance(NodeFinalPos(i)(i2), OPRFoots(i3)) <= NodeActivationDistance(i)(i2) Then 'Is there Right Foot close?
                        NodeOutputs(i)(i2) = True
                    ElseIf V3Distance(NodeFinalPos(i)(i2), OPLFoots(i3)) <= NodeActivationDistance(i)(i2) Then 'Is there Left Foot close?
                        NodeOutputs(i)(i2) = True
                    End If
                Next
            Next
        Next

        Select Case outputtestindex 'To test outputs of a device
            Case 1
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 0 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(0) = True
                End If
            Case 2
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 1 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(1) = True
                End If
            Case 3
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 2 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(2) = True
                End If
            Case 4
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 3 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(3) = True
                End If
            Case 5
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 4 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(4) = True
                End If
            Case 6
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 5 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(5) = True
                End If
            Case 7
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 6 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(6) = True
                End If
            Case 8
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 7 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(7) = True
                End If
            Case 9
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 8 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(8) = True
                End If
            Case 10
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 9 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(9) = True
                End If
            Case 11
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 10 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(10) = True
                End If
            Case 12
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 11 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(11) = True
                End If
            Case 13
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 12 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(12) = True
                End If
            Case 14
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 13 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(13) = True
                End If
            Case 15
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 14 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(14) = True
                End If
            Case 16
                If NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count > 15 Then
                    NodeOutputs(DGVDevice.SelectedCells(0).RowIndex)(15) = True
                End If
        End Select

        If outputtest = True Then   'If output test is on
            outputtestindex = outputtestindex + 1   'Cycle through all the inputs
            If outputtestindex = NodeOutputs(DGVDevice.SelectedCells(0).RowIndex).Count Then    'Reset it when it gets to the max of the device
                outputtestindex = 1
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
                        sendbytes(Prefix.Length + i2) = intensity 'Shift in output data for the device to use
                    Else
                        sendbytes(Prefix.Length + i2) = 0   'Else turn it off
                    End If
                Next
                Multicaster.Send(sendbytes, sendbytes.Length, ep)   'SEND TO THE DEVICE!
            Next
        End If


        If Enable3D = True Then 'If the Enable 3D is true
            GlControl1.Invalidate() 'Call the 3D to update
        End If
        GC.Collect() 'House Keeping
    End Sub

    '3D Distance math
    Function V3Distance(ByVal V1 As Vector3, ByVal V2 As Vector3)
        Dim Distance As Single = Math.Sqrt((V2.X - V1.X) ^ 2 + (V2.Y - V1.Y) ^ 2 + (V2.Z - V1.Z) ^ 2)   'Two Vector3 distance formula cause this version of OpenTK is old lol. 
        Return Math.Abs(Distance)   'Make it always positive cause we're not going for negative here
    End Function

    'GL-Control loaded
    Private Sub GlControl1_Load(sender As Object, e As EventArgs) Handles GlControl1.Load
        GLLoaded = True '3D is loaded
        GL.ClearColor(Color.Black) 'set the back color
    End Sub

    'GL-Control main routine, this like, draws the 3D part
    Private Sub GlControl1_Paint(sender As Object, e As PaintEventArgs) Handles GlControl1.Paint
        GL.Clear(ClearBufferMask.ColorBufferBit)    'Clear buffers
        GL.Clear(ClearBufferMask.DepthBufferBit)

        'Basic Setup for viewing
        Dim perspective As Matrix4 = Matrix4.CreatePerspectiveFieldOfView(1.04, 4 / 3, 0.01, 10000) 'Setup Perspective
        Dim lookat As Matrix4 = Matrix4.LookAt(zpos, ypos, 0, 0, ypos, 0, 0, 1, 0) 'Setup camera
        GL.MatrixMode(MatrixMode.Projection) 'Load Perspective
        GL.LoadIdentity()
        GL.LoadMatrix(perspective)
        GL.MatrixMode(MatrixMode.Modelview) 'Load Camera
        GL.LoadIdentity()
        GL.LoadMatrix(lookat)
        GL.Viewport(0, 0, GlControl1.Width, GlControl1.Height) 'Size of window
        GL.Enable(EnableCap.DepthTest) 'Enable correct Z Drawings
        GL.DepthFunc(DepthFunction.Less) 'Enable correct Z Drawings

        'Y is up, Z is twards you, X is left and right

        GL.Rotate(-rotateX / 4, 0, 0, 1)    'Mouse rotation of the 3D
        GL.Rotate(rotateY / 4, 0, 1, 0)

        GL.Rotate(slowrotateY, 0, 1, 0) 'Awwww yiss that slow rotate

        'Draw World Orientation, the 3 color 3 line thing at the bottom
        GL.LineWidth(3)
        GL.Begin(BeginMode.Lines)
        GL.Color3(Color.Red)
        GL.Vertex3(0, lower, 0)
        GL.Vertex3(0.3, lower, 0)
        GL.Color3(Color.Green)
        GL.Vertex3(0, lower, 0)
        GL.Vertex3(0, lower + 0.3, 0)
        GL.Color3(Color.Blue)
        GL.Vertex3(0, lower, 0)
        GL.Vertex3(0, lower, 0.3)
        GL.End()

        'Draw the bones
        GL.LineWidth(3)
        GL.Begin(BeginMode.Lines)
        GL.Color3(Color.White)

        GL.Vertex3(head)
        GL.Vertex3(chest)

        GL.Vertex3(chest)
        GL.Vertex3(hips)

        GL.Vertex3(chest)
        GL.Vertex3(RShoulder)

        GL.Vertex3(chest)
        GL.Vertex3(LShoulder)

        GL.Vertex3(RShoulder)
        GL.Vertex3(RLA)

        GL.Vertex3(RLA)
        GL.Vertex3(RHand)

        GL.Vertex3(LShoulder)
        GL.Vertex3(LLA)

        GL.Vertex3(LLA)
        GL.Vertex3(LHand)

        GL.Vertex3(hips)
        GL.Vertex3(RUL)

        GL.Vertex3(RUL)
        GL.Vertex3(RLL)

        GL.Vertex3(RLL)
        GL.Vertex3(RFoot)

        GL.Vertex3(hips)
        GL.Vertex3(LUL)

        GL.Vertex3(LUL)
        GL.Vertex3(LLL)

        GL.Vertex3(LLL)
        GL.Vertex3(LFoot)

        GL.End() 'Finish the begin mode with "end"

        'Draw head direction
        GL.Color3(Color.Blue)
        GL.Begin(BeginMode.Lines)
        GL.Vertex3(head)
        GL.Vertex3(HeadVector)
        GL.End()

        'Draw Hands
        GL.Color3(Color.Blue)
        GL.Begin(BeginMode.Triangles)
        GL.Vertex3(RHand)
        GL.Vertex3(RHandV1)
        GL.Vertex3(RHandV2)
        GL.Vertex3(LHand)
        GL.Vertex3(LHandV1)
        GL.Vertex3(LHandV2)
        GL.End()

        'Draw the local player points
        GL.PointSize(10)
        GL.Begin(BeginMode.Points)
        GL.Color3(Color.Blue)
        GL.Vertex3(head)
        GL.Vertex3(hips)
        GL.Vertex3(chest)
        GL.Vertex3(RShoulder)
        GL.Vertex3(LShoulder)
        GL.Vertex3(RLA)
        GL.Vertex3(LLA)
        GL.Vertex3(RUL)
        GL.Vertex3(LUL)
        GL.Vertex3(RLL)
        GL.Vertex3(LLL)
        GL.Vertex3(RFoot)
        GL.Vertex3(LFoot)
        GL.Color3(Color.Green)
        GL.Vertex3(RHand)
        GL.Vertex3(RFoot)
        GL.Color3(Color.Red)
        GL.Vertex3(LHand)
        GL.Vertex3(LFoot)
        GL.End() 'Finish the begin mode with "end"

        'Draw the other player points
        For i = 0 To OtherPlayers.Count - 1
            GL.PointSize(5)
            GL.Begin(BeginMode.Points)
            GL.Color3(Color.LightGray)
            GL.Vertex3(OPHeads(i))
            GL.Vertex3(OPHipss(i))
            GL.Vertex3(OPLFoots(i))
            GL.Vertex3(OPRFoots(i))
            GL.Color3(Color.Green)
            GL.Vertex3(OPRTDs(i))
            GL.Vertex3(OPRIDs(i))
            GL.Color3(Color.Red)
            GL.Vertex3(OPLTDs(i))
            GL.Vertex3(OPLIDs(i))
            GL.End() 'Finish the begin mode with "end"
        Next

        'Draw Nodes
        For i2 = 0 To NodeDeviceNames.Count - 1
            For i = 0 To NodeFinalPos(i2).Count - 1 'This section shifts the nodes to be based off the root bone that they've been defined for
                If DGVDevice.SelectedCells(0).RowIndex = i2 And DGVNodes.SelectedCells(0).RowIndex = i Then
                    GL.LineWidth(10)
                    Dim directionVectorX As New Vector3(NodeActivationDistance(i2)(i) * 2, 0, 0)
                    Dim directionVectorY As New Vector3(0, NodeActivationDistance(i2)(i) * 2, 0)
                    Dim directionVectorZ As New Vector3(0, 0, NodeActivationDistance(i2)(i) * 2)
                    Select Case NodeRootBone(i2)(i)
                        Case 1
                            directionVectorX = Vector3.TransformVector(directionVectorX, HeadRM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, HeadRM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, HeadRM)
                        Case 2
                            directionVectorX = Vector3.TransformVector(directionVectorX, HipsRM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, HipsRM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, HipsRM)
                        Case 3
                            directionVectorX = Vector3.TransformVector(directionVectorX, ChestRM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, ChestRM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, ChestRM)
                        Case 4
                            directionVectorX = Vector3.TransformVector(directionVectorX, RShoulderM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, RShoulderM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, RShoulderM)
                        Case 5
                            directionVectorX = Vector3.TransformVector(directionVectorX, LShoulderM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, LShoulderM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, LShoulderM)
                        Case 6
                            directionVectorX = Vector3.TransformVector(directionVectorX, RLAM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, RLAM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, RLAM)
                        Case 7
                            directionVectorX = Vector3.TransformVector(directionVectorX, LLAM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, LLAM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, LLAM)
                        Case 8
                            directionVectorX = Vector3.TransformVector(directionVectorX, RHandRM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, RHandRM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, RHandRM)
                        Case 9
                            directionVectorX = Vector3.TransformVector(directionVectorX, LHandRM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, LHandRM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, LHandRM)
                        Case 10
                            directionVectorX = Vector3.TransformVector(directionVectorX, RULM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, RULM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, RULM)
                        Case 11
                            directionVectorX = Vector3.TransformVector(directionVectorX, LULM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, LULM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, LULM)
                        Case 12
                            directionVectorX = Vector3.TransformVector(directionVectorX, RLLM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, RLLM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, RLLM)
                        Case 13
                            directionVectorX = Vector3.TransformVector(directionVectorX, LLLM)
                            directionVectorY = Vector3.TransformVector(directionVectorY, LLLM)
                            directionVectorZ = Vector3.TransformVector(directionVectorZ, LLLM)
                    End Select

                    Select Case DGVNodes.SelectedCells(0).ColumnIndex   'This is the small colored line that appears on the node when a direction is selected
                        Case 2
                            GL.Color3(Color.Red)
                            GL.Begin(BeginMode.Lines)
                            GL.Vertex3(NodeFinalPos(i2)(i))
                            GL.Vertex3(NodeFinalPos(i2)(i) + directionVectorX)
                            GL.End()
                        Case 3
                            GL.Color3(Color.Green)
                            GL.Begin(BeginMode.Lines)
                            GL.Vertex3(NodeFinalPos(i2)(i))
                            GL.Vertex3(NodeFinalPos(i2)(i) + directionVectorY)
                            GL.End()
                        Case 4
                            GL.Color3(Color.Blue)
                            GL.Begin(BeginMode.Lines)
                            GL.Vertex3(NodeFinalPos(i2)(i))
                            GL.Vertex3(NodeFinalPos(i2)(i) + directionVectorZ)
                            GL.End()
                    End Select
                End If
                GL.LineWidth(1)

                If NodeOutputs(i2)(i) = True Then   'If the node is on, color it light blue vs orange
                    GL.Color3(Color.Green)
                ElseIf DGVDevice.SelectedCells(0).RowIndex = i2 And DGVNodes.SelectedCells(0).RowIndex = i Then
                    GL.Color3(Color.LightBlue)
                Else
                    GL.Color3(Color.Orange)
                End If

                For i3 = 0 To 15 'This following mess creates the lined spheres for the nodes
                    GL.Begin(BeginMode.LineLoop)
                    Dim tempangi As Single = (i3 / 15) * 360 * (Math.PI / 180)
                    Dim yi As Single = Math.Sin(tempangi) * (NodeActivationDistance(i2)(i))
                    Dim xi As Single = Math.Cos(tempangi)
                    For i4 = 0 To 15
                        Dim tempangi2 As Single = (i4 / 15) * 360 * (Math.PI / 180)
                        Dim xi2 As Single = Math.Cos(tempangi2) * (NodeActivationDistance(i2)(i)) * xi
                        Dim zi2 As Single = Math.Sin(tempangi2) * (NodeActivationDistance(i2)(i)) * xi
                        GL.Vertex3(xi2 + NodeFinalPos(i2)(i).X, yi + NodeFinalPos(i2)(i).Y, zi2 + NodeFinalPos(i2)(i).Z)
                    Next
                    GL.End()
                Next

            Next
        Next

        GL.End() 'Finish the begin mode with "end"



        'Add Avatar
        'I like apologize for this, its to get a rough guess at where your avatar body looks like compared to the bones
        If AvatarLoaded = True Then 'Make sure there's an avatar first
            GL.Enable(EnableCap.Light0) 'This follwing area makes it mostly transparent
            GL.Enable(EnableCap.Lighting)
            GL.ShadeModel(ShadingModel.Smooth)
            Dim params1() As Single = {0.2F, 0.2F, 0.2F, 1}
            Dim params2() As Single = {0, 0, 0}
            GL.LightModel(OpenTK.Graphics.OpenGL.LightModelParameter.LightModelAmbient, params1)
            GL.Light(OpenTK.Graphics.OpenGL.LightName.Light0, OpenTK.Graphics.OpenGL.LightParameter.Position, params2)

            GL.Enable(EnableCap.Blend)
            GL.BlendFunc(BlendingFactorSrc.OneMinusConstantColorExt, BlendingFactorDest.One) 'BlendingFactorSrc.OneMinusDstColor, BlendingFactorDest.DstColor)

            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.AmbientAndDiffuse, Color.LightBlue)

            GL.PushMatrix() 'This section rotates it very loosely to the correct spot
            GL.Translate(0, lower, 0)
            GL.Rotate(AvatarRotation.X, 1, 0, 0)
            GL.Rotate(AvatarRotation.Y, 0, 1, 0)
            GL.Rotate(AvatarRotation.Z, 0, 0, 1)
            GL.Begin(BeginMode.Triangles)

            For i = 0 To Avatarpoints.Count - 1 Step 3  'This part draws the mesh faces of the avatar
                GL.Normal3(Avatarnormals(i / 3))
                GL.Vertex3(Avatarpoints(i))
                GL.Vertex3(Avatarpoints(i + 1))
                GL.Vertex3(Avatarpoints(i + 2))
            Next
            GL.End()
            GL.PopMatrix()

            GL.Disable(EnableCap.Light0)
            GL.Disable(EnableCap.Lighting)
        End If

        'Finally...
        GraphicsContext.CurrentContext.VSync = True 'Caps frame rate as to not over run GPU
        GlControl1.SwapBuffers() 'Takes from the 'GL' and puts into control

    End Sub

    '3D control when you mouse down on the 3D
    Private Sub GlControl1_MouseDown(sender As Object, e As MouseEventArgs) Handles GlControl1.MouseDown
        mousecontrol = True 'Set we have moused down
        If e.Button = MouseButtons.Right Then   'Right click for vertical positioning
            mousecontroltype = 2
            mousestartposition = New Point(MousePosition.X - positionX, MousePosition.Y - positionY)
        ElseIf e.Button = MouseButtons.Left Then 'Left click for rotation
            mousestartrotate = New Point(MousePosition.X - rotateY, MousePosition.Y - rotateX)
            mousecontroltype = 1
        End If
        slowrotate = False 'Turn off the slow rotate
    End Sub

    'Mouse movement on 3D
    Private Sub GlControl1_MouseMove(sender As Object, e As MouseEventArgs) Handles GlControl1.MouseMove
        If mousecontrol = True Then 'Only if we are actively moving the 3D, or else it lags the program

            If mousecontroltype = 1 Then 'For rotation
                rotateY = (MousePosition.X - mousestartrotate.X)
                rotateX = (MousePosition.Y - mousestartrotate.Y)
                GlControl1.Invalidate() 'Reset the 3D
            ElseIf mousecontroltype = 2 Then 'For vertical position
                ypos = (MousePosition.Y - mousestartposition.Y) / 400
                GlControl1.Invalidate() 'Reset the 3D
            End If

        End If

    End Sub

    'Mouse has ended 3D control
    Private Sub GlControl1_MouseUp(sender As Object, e As MouseEventArgs) Handles GlControl1.MouseUp
        mousecontrol = False 'No more mousing
        mousecontroltype = 0 'No more controling
        GlControl1.Invalidate() 'Update the 3D
    End Sub

    'Mouse event for 3D zoom
    Private Sub GlControl1_MouseWheel(sender As Object, e As MouseEventArgs) Handles GlControl1.MouseWheel
        zpos = zpos + (e.Delta / 1000) 'e.delta being the scoll amount, which is like... 3 by default I think
        If zpos < 0 Then 'Put a minimum on it
            zpos = 0
        End If

        GlControl1.Invalidate() 'Reset the 3D
    End Sub

    'For reading in an avatar's STL
    Public Sub fileimportSTL() 'First you should know, this requires the normals as well
        Dim pointsstring() As Byte  'Bytes of STL file

        GC.Collect()    'Do this cause file reading is odd
        pointsstring = System.IO.File.ReadAllBytes(Avatarlocation)  'Read bytes from file
        GC.Collect()

        Avatarcolors = Color.Cyan   'Color, but I don't think it maters
        Dim pointcount As UInt32 = BitConverter.ToUInt32(pointsstring, 80)  'Get total points
        Dim NX, NY, NZ, X1, X2, X3, Y1, Y2, Y3, Z1, Z2, Z3 As Single

        For i = 84 To pointsstring.Length - 51 Step 50

            NX = BitConverter.ToSingle(pointsstring, i + 0)
            NY = BitConverter.ToSingle(pointsstring, i + 4)
            NZ = BitConverter.ToSingle(pointsstring, i + 8)

            Avatarnormals.Add(New Vector3(NX, NY, NZ))  'Get normals (which way face faces!)

            X1 = BitConverter.ToSingle(pointsstring, i + 12)
            Y1 = BitConverter.ToSingle(pointsstring, i + 16)
            Z1 = BitConverter.ToSingle(pointsstring, i + 20)

            X2 = BitConverter.ToSingle(pointsstring, i + 24)
            Y2 = BitConverter.ToSingle(pointsstring, i + 28)
            Z2 = BitConverter.ToSingle(pointsstring, i + 32)

            X3 = BitConverter.ToSingle(pointsstring, i + 36)
            Y3 = BitConverter.ToSingle(pointsstring, i + 40)
            Z3 = BitConverter.ToSingle(pointsstring, i + 44)

            Avatarpoints.Add(New Vector3(X1, Y1, Z1))  'Add points to lists
            Avatarpoints.Add(New Vector3(X2, Y2, Z2))
            Avatarpoints.Add(New Vector3(X3, Y3, Z3))
        Next
        AvatarLoaded = True 'Load Avatar into drawing
    End Sub

    'Generic Avatar Rotates
    Private Sub X90ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles X90ToolStripMenuItem1.Click
        AvatarRotation.X = AvatarRotation.X - 90
    End Sub
    Private Sub Y90ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles Y90ToolStripMenuItem.Click
        AvatarRotation.Y = AvatarRotation.Y + 90
    End Sub
    Private Sub Y90ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles Y90ToolStripMenuItem1.Click
        AvatarRotation.Y = AvatarRotation.Y - 90
    End Sub
    Private Sub Z90ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles Z90ToolStripMenuItem.Click
        AvatarRotation.Z = AvatarRotation.Z + 90
    End Sub
    Private Sub Z90ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles Z90ToolStripMenuItem1.Click
        AvatarRotation.Z = AvatarRotation.Z - 90
    End Sub
    Private Sub X90ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles X90ToolStripMenuItem.Click
        AvatarRotation.X = AvatarRotation.X + 90
    End Sub

    'Delete Device
    Private Sub RemoveDeviceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RemoveDeviceToolStripMenuItem.Click
        If DGVDevice.SelectedCells.Count > 0 Then
            If DGVDevice.SelectedCells(0).RowIndex <> -1 Then 'For the specific listed device
                If MessageBox.Show("Delete " & NodeDeviceNames(DGVDevice.SelectedCells(0).RowIndex), "Really?", MessageBoxButtons.YesNo) = DialogResult.Yes Then 'Confirm deletion
                    NodeDeviceNames.RemoveAt(DGVDevice.SelectedCells(0).RowIndex) 'Remove device from lists
                    NodeDeviceConnection.RemoveAt(DGVDevice.SelectedCells(0).RowIndex)
                    NodeOutputs.RemoveAt(DGVDevice.SelectedCells(0).RowIndex)
                    NodeBoneOffset.RemoveAt(DGVDevice.SelectedCells(0).RowIndex)
                    NodeFinalPos.RemoveAt(DGVDevice.SelectedCells(0).RowIndex)
                    NodeRootBone.RemoveAt(DGVDevice.SelectedCells(0).RowIndex)
                    NodeActivationDistance.RemoveAt(DGVDevice.SelectedCells(0).RowIndex)
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
            DGVNodes.Rows(DGVNodes.Rows.Count - 1).Cells(1).Value = RootBones(NodeRootBone(DGVDevice.SelectedCells(0).RowIndex)(i))
            DGVNodes.Rows(DGVNodes.Rows.Count - 1).Cells(2).Value = NodeBoneOffset(DGVDevice.SelectedCells(0).RowIndex)(i).X
            DGVNodes.Rows(DGVNodes.Rows.Count - 1).Cells(3).Value = NodeBoneOffset(DGVDevice.SelectedCells(0).RowIndex)(i).Y
            DGVNodes.Rows(DGVNodes.Rows.Count - 1).Cells(4).Value = NodeBoneOffset(DGVDevice.SelectedCells(0).RowIndex)(i).Z
            DGVNodes.Rows(DGVNodes.Rows.Count - 1).Cells(5).Value = NodeActivationDistance(DGVDevice.SelectedCells(0).RowIndex)(i)
        Next
        DGVupdating = False 'This prevents the cell value changed event from happening cause its all original data
    End Sub

    'Save bone
    Private Sub DGVNodes_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles DGVNodes.CellValueChanged
        If DGVupdating = False Then 'Check we're not just getting the original data
            If e.RowIndex <> -1 Then
                NodeRootBone(DGVDevice.SelectedCells(0).RowIndex)(e.RowIndex) = RootBones.IndexOf(DGVNodes.Rows(e.RowIndex).Cells(1).Value) 'If root bone changed, save that
                Try
                    NodeBoneOffset(DGVDevice.SelectedCells(0).RowIndex)(e.RowIndex) = New Vector3(DGVNodes.Rows(e.RowIndex).Cells(2).Value, DGVNodes.Rows(e.RowIndex).Cells(3).Value, DGVNodes.Rows(e.RowIndex).Cells(4).Value) 'If offset changed, save that
                    DGVNodes.Rows(e.RowIndex).Cells(2).Style.BackColor = Color.White
                    DGVNodes.Rows(e.RowIndex).Cells(3).Style.BackColor = Color.White
                    DGVNodes.Rows(e.RowIndex).Cells(4).Style.BackColor = Color.White
                Catch ex As Exception
                    DGVNodes.Rows(e.RowIndex).Cells(2).Style.BackColor = Color.Red 'If the math conversion failed, set it to red so user can fix
                    DGVNodes.Rows(e.RowIndex).Cells(3).Style.BackColor = Color.Red
                    DGVNodes.Rows(e.RowIndex).Cells(4).Style.BackColor = Color.Red
                End Try
                Try
                    NodeActivationDistance(DGVDevice.SelectedCells(0).RowIndex)(e.RowIndex) = DGVNodes.Rows(e.RowIndex).Cells(5).Value
                    DGVNodes.Rows(e.RowIndex).Cells(5).Style.BackColor = Color.White
                Catch ex As Exception
                    DGVNodes.Rows(e.RowIndex).Cells(5).Style.BackColor = Color.Red
                End Try
                Settingschanged = True 'Something changed, prompt at exit for save
            End If
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
        If NodeDeviceNames.Count <> 0 Then
            If MessageBox.Show("Overwrite current settings?", "But wait", MessageBoxButtons.OKCancel) = MessageBoxButtons.OK Then
                OFDSettings.ShowDialog()    'Load the Dialog
            End If
        Else
            OFDSettings.ShowDialog()    'Load the Dialog
        End If

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
        NodeRootBone.Clear()
        NodeBoneOffset.Clear()
        NodeFinalPos.Clear()
        NodeActivationDistance.Clear()

        DGVDevice.Rows.Clear()

        Dim readstring() As String  'Define the string to read into

        GC.Collect()    'Garbage collection probably correct right?
        readstring = System.IO.File.ReadAllLines(OFDSettings.FileName) 'Read the file into the string array!
        GC.Collect()    'I mean, its probably the worst thing I could do here

        If readstring(0) >= 1 Then   'Version control! Other versions can have elseif >=2 or something
            intensity = readstring(1)
            Dim DeviceCount As Integer = readstring(2)
            Dim vectorsplit() As String 'Split the vector to 3 parts
            Dim linesread As Integer = 3    'Math is hard
            For i = 0 To DeviceCount - 1
                NodeDeviceNames.Add(readstring(linesread))
                NodeOutputs.Add(New List(Of Boolean))
                For i2 = 0 To Int(readstring(1 + linesread)) - 1
                    NodeOutputs(NodeOutputs.Count - 1).Add(False)
                Next
                'NodeDeviceConnection.Add(readstring(2 + linesread))
                NodeDeviceConnection.Add(0)
                linesread = linesread + 3
                NodeRootBone.Add(New List(Of Integer))
                NodeBoneOffset.Add(New List(Of OpenTK.Vector3))
                NodeFinalPos.Add(New List(Of OpenTK.Vector3))
                NodeActivationDistance.Add(New List(Of Single))
                For i2 = 0 To NodeOutputs(i).Count - 1
                    NodeRootBone(NodeDeviceNames.Count - 1).Add(readstring(linesread))
                    linesread = linesread + 1
                Next
                For i2 = 0 To NodeOutputs(i).Count - 1
                    vectorsplit = readstring(linesread).Split(",")
                    NodeBoneOffset(NodeDeviceNames.Count - 1).Add(New Vector3(vectorsplit(0), vectorsplit(1), vectorsplit(2)))
                    NodeFinalPos(NodeDeviceNames.Count - 1).Add(New OpenTK.Vector3(0, 0, 0))
                    linesread = linesread + 1
                Next
                For i2 = 0 To NodeOutputs(i).Count - 1
                    NodeActivationDistance(NodeDeviceNames.Count - 1).Add(readstring(linesread))
                    linesread = linesread + 1
                Next

                TrackBar1.Value = intensity
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

        WriteList.Add("1")  'Version number
        WriteList.Add(intensity)
        WriteList.Add(NodeDeviceNames.Count.ToString)
        For i = 0 To NodeDeviceNames.Count - 1
            WriteList.Add(NodeDeviceNames(i))
            WriteList.Add(NodeOutputs(i).Count)
            WriteList.Add(NodeDeviceConnection(i))
            For i2 = 0 To NodeOutputs(i).Count - 1
                WriteList.Add(NodeRootBone(i)(i2).ToString)
            Next
            For i2 = 0 To NodeOutputs(i).Count - 1
                WriteList.Add(NodeBoneOffset(i)(i2).X & "," & NodeBoneOffset(i)(i2).Y & "," & NodeBoneOffset(i)(i2).Z)
            Next
            For i2 = 0 To NodeOutputs(i).Count - 1
                WriteList.Add(NodeActivationDistance(i)(i2))
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

    'Slow rotate enable
    Private Sub SpinToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SpinToolStripMenuItem.Click
        slowrotate = True
    End Sub

    'Setup device on wifi show dialog
    Private Sub SetWifiOnDeviceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SetWifiOnDeviceToolStripMenuItem.Click
        DeviceWifi.Show()
    End Sub

    'Intensity scroll
    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        intensity = TrackBar1.Value
        Settingschanged = True
    End Sub

    'Test outputs toggle
    Private Sub TestDeviceOutputsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TestDeviceOutputsToolStripMenuItem.Click
        If outputtest = True Then
            outputtest = False
            TestDeviceOutputsToolStripMenuItem.Checked = False
        Else
            outputtest = True
            outputtestindex = 1
            TestDeviceOutputsToolStripMenuItem.Checked = True
        End If
    End Sub

    'Form closing, request save if something changed
    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If Settingschanged = True Then
            If MessageBox.Show("Save Settings?", "Save", MessageBoxButtons.YesNo) = DialogResult.Yes Then   'Prompt for file save
                SFDSettings.ShowDialog()    'If yes
            End If
        End If
    End Sub
End Class
