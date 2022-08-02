Public Class DeviceEditor
    Private Sub BTNApply_Click(sender As Object, e As EventArgs) Handles BTNApply.Click
        If Form1.DeviceMethod = 1 Then
            Form1.NodeDeviceNames.Add(TextBox1.Text)
            Form1.NodeDeviceConnection.Add(0)
            Form1.NodeOutputs.Add(New List(Of Boolean))
            Form1.NodeNames.Add(New List(Of String))

            Form1.OctoMotor.Add(New List(Of Integer)) 'OctoSlime Additions
            Form1.OctoNode.Add(New List(Of Integer))

            For i = 0 To NumericUpDown1.Value - 1
                Form1.NodeOutputs(Form1.NodeOutputs.Count - 1).Add(False)
            Next
            Form1.NodeForce.Add(New List(Of Integer))
            For i = 0 To NumericUpDown1.Value - 1
                Form1.NodeNames(Form1.NodeDeviceNames.Count - 1).Add("Haptic")
                Form1.NodeForce(Form1.NodeDeviceNames.Count - 1).Add(100)
            Next

            For i = 0 To 7 'Add OctoSlime numbers
                Form1.OctoMotor(Form1.NodeDeviceNames.Count - 1).Add(1)
                Form1.OctoNode(Form1.NodeDeviceNames.Count - 1).Add(1)

            Next

            Form1.DGVDevice.Rows.Add()
            Form1.DGVDevice.Rows(Form1.DGVDevice.Rows.Count - 1).Cells(0).Value = Form1.NodeDeviceNames.Count
            Form1.DGVDevice.Rows(Form1.DGVDevice.Rows.Count - 1).Cells(1).Value = Form1.NodeDeviceNames(Form1.NodeDeviceNames.Count - 1)
            Form1.DGVNodeUpdate()
            Me.Close()
        ElseIf Form1.DeviceMethod = 2 Then
            Form1.NodeDeviceNames(Form1.DeviceIndex) = TextBox1.Text
            'Form1.NodeDeviceConnection(Form1.DeviceIndex) = TextBox2.Text
            If NumericUpDown1.Value > Form1.NodeOutputs(Form1.DeviceIndex).Count Then
                For i = Form1.NodeOutputs(Form1.DeviceIndex).Count To NumericUpDown1.Value - 1
                    Form1.NodeOutputs(Form1.DeviceIndex).Add(False)
                    Form1.NodeNames(Form1.DeviceIndex).Add("Haptic")
                    Form1.NodeForce(Form1.DeviceIndex).Add(0)
                Next
            ElseIf NumericUpDown1.Value < Form1.NodeOutputs(Form1.DeviceIndex).count Then
                Dim difference As Integer = Form1.NodeOutputs(Form1.DeviceIndex).Count - NumericUpDown1.Value
                For i = 1 To difference
                    Form1.NodeNames(Form1.DeviceIndex).RemoveAt(Form1.NodeNames(Form1.DeviceIndex).Count - 1)
                    Form1.NodeOutputs(Form1.DeviceIndex).Remove(0)
                    Form1.NodeForce(Form1.DeviceIndex).RemoveAt(Form1.NodeForce(Form1.DeviceIndex).Count - 1)
                Next
            End If
            Form1.DGVDevicesUpdate()
            Form1.DGVNodeUpdate()

            Form1.Settingschanged = True
            Me.Close()
        End If
    End Sub

    Private Sub BTNCancel_Click(sender As Object, e As EventArgs) Handles BTNCancel.Click
        Me.Close()
    End Sub

    Private Sub DeviceEditor_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'OctoSlime setup
        If Form1.OctoSlimeMode = True Then
            Label1.Text = "IP Address"
            Label3.Text = "Motors Count"
        Else
            Label1.Text = "Device Name"
            Label3.Text = "Nodes On Device"
        End If

        If Form1.DeviceMethod = 1 Then
            TextBox1.Text = ""
            'TextBox2.Text = ""
            NumericUpDown1.Value = 0
        ElseIf Form1.DeviceMethod = 2 Then
            TextBox1.Text = Form1.NodeDeviceNames(Form1.DeviceIndex)
            'TextBox2.Text = Form1.NodeDeviceConnection(Form1.DeviceIndex)
            NumericUpDown1.Value = Form1.NodeOutputs(Form1.DeviceIndex).Count
        End If
    End Sub
End Class