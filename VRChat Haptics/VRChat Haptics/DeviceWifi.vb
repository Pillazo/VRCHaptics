Imports System.ComponentModel
Imports System.IO.Ports

Public Class DeviceWifi

    Dim SerialList As String() = System.IO.Ports.SerialPort.GetPortNames()
    Dim port As String

    Private Sub DeviceWifi_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SerialList = System.IO.Ports.SerialPort.GetPortNames()
        For Each Me.port In SerialList
            CBPort.Items.Add(port)
        Next port
        If CBPort.Items.Count = 0 Then
            MsgBox("Not Serial port found")
        End If
        Try
            SerialPort1.PortName = CBPort.Text
            SerialPort1.Open()
            SerialPort1.DtrEnable = True
        Catch ex As Exception
            CBPort.Items.Clear()
            For Each Me.port In SerialList
                CBPort.Items.Add(port)
            Next port
            If CBPort.Items.Count > 0 Then
                CBPort.Text = CBPort.Items(0)
            End If
        End Try
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("https://www.ftdichip.com/Drivers/VCP.htm")
    End Sub

    Private Sub ButtonConnect_Click(sender As Object, e As EventArgs) Handles ButtonConnect.Click
        Try
            SerialPort1.PortName = CBPort.Text
            SerialPort1.Open()
            SerialPort1.DtrEnable = True
            ButtonConnect.BackColor = Color.Green
            ButtonConnect.ForeColor = Color.White
        Catch ex As Exception
            MsgBox("Wrong port, try another!")
        End Try
    End Sub

    Private Sub ButtonSendWifi_Click(sender As Object, e As EventArgs) Handles ButtonSendWifi.Click
        If SerialPort1.IsOpen = True Then
            SerialPort1.Write("Wifi" & TextBox1.Text & "^" & TextBox2.Text & "&" & TextBox3.Text)

        End If
    End Sub

    Private Sub ButtonClose_Click(sender As Object, e As EventArgs) Handles ButtonClose.Click
        Me.Close()
    End Sub

    Private Sub SerialPort1_DataReceived(sender As Object, e As SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        'Serial Recieve
        Dim comBuffer As Byte()
        If SerialPort1.IsOpen = True Then
            Try
                Dim n As Integer = SerialPort1.BytesToRead
                comBuffer = New Byte(n - 1) {}
                SerialPort1.Read(comBuffer, 0, n)
                If n = 0 Then
                    Return
                End If
                Dim responcestring As String = System.Text.Encoding.ASCII.GetString(comBuffer)
                RichTextBox1.Invoke(Sub() RichTextBox1.AppendText(responcestring))

                If responcestring.Contains("WiFi connected!") Then
                    ButtonSendWifi.Invoke(Sub() ButtonSendWifi.BackColor = Color.Green)
                    ButtonSendWifi.Invoke(Sub() ButtonSendWifi.ForeColor = Color.White)
                    ButtonConnect.Invoke(Sub() ButtonConnect.BackColor = Color.LightGray)
                    ButtonConnect.Invoke(Sub() ButtonConnect.ForeColor = Color.Black)
                    SerialPort1.Close()
                End If
            Catch ex As Exception
                'goterror = True
            End Try
        End If
    End Sub

    Private Sub DeviceWifi_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Form1.outputtest = True
    End Sub
End Class