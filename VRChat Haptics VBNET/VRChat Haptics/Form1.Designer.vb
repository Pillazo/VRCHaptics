<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.MainTimer = New System.Windows.Forms.Timer(Me.components)
        Me.GlControl1 = New OpenTK.GLControl()
        Me.LblLogStatus = New System.Windows.Forms.Label()
        Me.RetryTimer = New System.Windows.Forms.Timer(Me.components)
        Me.DGVNodes = New System.Windows.Forms.DataGridView()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.DGVDevice = New System.Windows.Forms.DataGridView()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LoadDeviceNodeDescriptionToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SaveDeviceNodeDescriptionToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CloseToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DeviceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AddDeviceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EditDeviceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RemoveDeviceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SetWifiOnDeviceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TestDeviceOutputsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AvatarToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LoadAvatarToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RotateToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.X90ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.X90ToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.Y90ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Y90ToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.Z90ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Z90ToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.DRenderToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.KeepAtFrontToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SpinToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.OFDAvatar = New System.Windows.Forms.OpenFileDialog()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.OFDSettings = New System.Windows.Forms.OpenFileDialog()
        Me.SFDSettings = New System.Windows.Forms.SaveFileDialog()
        Me.Output = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.RootBone = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.X = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Y = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Z = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Activation = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Force = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Test = New System.Windows.Forms.DataGridViewButtonColumn()
        CType(Me.DGVNodes, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        CType(Me.DGVDevice, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.MenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'MainTimer
        '
        Me.MainTimer.Interval = 10
        '
        'GlControl1
        '
        Me.GlControl1.BackColor = System.Drawing.Color.Black
        Me.GlControl1.Location = New System.Drawing.Point(357, 12)
        Me.GlControl1.Name = "GlControl1"
        Me.GlControl1.Size = New System.Drawing.Size(395, 510)
        Me.GlControl1.TabIndex = 47
        Me.GlControl1.VSync = False
        '
        'LblLogStatus
        '
        Me.LblLogStatus.AutoSize = True
        Me.LblLogStatus.Location = New System.Drawing.Point(12, 36)
        Me.LblLogStatus.Name = "LblLogStatus"
        Me.LblLogStatus.Size = New System.Drawing.Size(58, 13)
        Me.LblLogStatus.TabIndex = 49
        Me.LblLogStatus.Text = "Log Status"
        '
        'RetryTimer
        '
        Me.RetryTimer.Interval = 5000
        '
        'DGVNodes
        '
        Me.DGVNodes.AllowUserToAddRows = False
        Me.DGVNodes.AllowUserToDeleteRows = False
        Me.DGVNodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DGVNodes.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Output, Me.RootBone, Me.X, Me.Y, Me.Z, Me.Activation, Me.Force, Me.Test})
        Me.DGVNodes.Location = New System.Drawing.Point(5, 19)
        Me.DGVNodes.Name = "DGVNodes"
        Me.DGVNodes.RowHeadersVisible = False
        Me.DGVNodes.Size = New System.Drawing.Size(338, 267)
        Me.DGVNodes.TabIndex = 51
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.DGVNodes)
        Me.GroupBox1.Location = New System.Drawing.Point(2, 230)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(349, 292)
        Me.GroupBox1.TabIndex = 54
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Nodes"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.DGVDevice)
        Me.GroupBox2.Location = New System.Drawing.Point(2, 90)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(349, 134)
        Me.GroupBox2.TabIndex = 55
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Device"
        '
        'DGVDevice
        '
        Me.DGVDevice.AllowUserToAddRows = False
        Me.DGVDevice.AllowUserToDeleteRows = False
        Me.DGVDevice.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DGVDevice.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn1, Me.DataGridViewTextBoxColumn2})
        Me.DGVDevice.Location = New System.Drawing.Point(5, 19)
        Me.DGVDevice.Name = "DGVDevice"
        Me.DGVDevice.ReadOnly = True
        Me.DGVDevice.RowHeadersVisible = False
        Me.DGVDevice.Size = New System.Drawing.Size(338, 109)
        Me.DGVDevice.TabIndex = 52
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.HeaderText = "Index"
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        Me.DataGridViewTextBoxColumn1.Width = 50
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.DataGridViewTextBoxColumn2.HeaderText = "Name"
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = True
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.DeviceToolStripMenuItem, Me.AvatarToolStripMenuItem, Me.DRenderToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(755, 24)
        Me.MenuStrip1.TabIndex = 56
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.LoadDeviceNodeDescriptionToolStripMenuItem, Me.SaveDeviceNodeDescriptionToolStripMenuItem, Me.CloseToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'LoadDeviceNodeDescriptionToolStripMenuItem
        '
        Me.LoadDeviceNodeDescriptionToolStripMenuItem.Name = "LoadDeviceNodeDescriptionToolStripMenuItem"
        Me.LoadDeviceNodeDescriptionToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.LoadDeviceNodeDescriptionToolStripMenuItem.Text = "Load Settings"
        '
        'SaveDeviceNodeDescriptionToolStripMenuItem
        '
        Me.SaveDeviceNodeDescriptionToolStripMenuItem.Name = "SaveDeviceNodeDescriptionToolStripMenuItem"
        Me.SaveDeviceNodeDescriptionToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.SaveDeviceNodeDescriptionToolStripMenuItem.Text = "Save Settings"
        '
        'CloseToolStripMenuItem
        '
        Me.CloseToolStripMenuItem.Name = "CloseToolStripMenuItem"
        Me.CloseToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.CloseToolStripMenuItem.Text = "Close"
        '
        'DeviceToolStripMenuItem
        '
        Me.DeviceToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AddDeviceToolStripMenuItem, Me.EditDeviceToolStripMenuItem, Me.RemoveDeviceToolStripMenuItem, Me.SetWifiOnDeviceToolStripMenuItem, Me.TestDeviceOutputsToolStripMenuItem})
        Me.DeviceToolStripMenuItem.Name = "DeviceToolStripMenuItem"
        Me.DeviceToolStripMenuItem.Size = New System.Drawing.Size(54, 20)
        Me.DeviceToolStripMenuItem.Text = "Device"
        '
        'AddDeviceToolStripMenuItem
        '
        Me.AddDeviceToolStripMenuItem.Name = "AddDeviceToolStripMenuItem"
        Me.AddDeviceToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.AddDeviceToolStripMenuItem.Text = "Add Device"
        '
        'EditDeviceToolStripMenuItem
        '
        Me.EditDeviceToolStripMenuItem.Name = "EditDeviceToolStripMenuItem"
        Me.EditDeviceToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.EditDeviceToolStripMenuItem.Text = "Edit Device"
        '
        'RemoveDeviceToolStripMenuItem
        '
        Me.RemoveDeviceToolStripMenuItem.Name = "RemoveDeviceToolStripMenuItem"
        Me.RemoveDeviceToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.RemoveDeviceToolStripMenuItem.Text = "Remove Device"
        '
        'SetWifiOnDeviceToolStripMenuItem
        '
        Me.SetWifiOnDeviceToolStripMenuItem.Name = "SetWifiOnDeviceToolStripMenuItem"
        Me.SetWifiOnDeviceToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.SetWifiOnDeviceToolStripMenuItem.Text = "Set Wifi on Device"
        '
        'TestDeviceOutputsToolStripMenuItem
        '
        Me.TestDeviceOutputsToolStripMenuItem.Name = "TestDeviceOutputsToolStripMenuItem"
        Me.TestDeviceOutputsToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.TestDeviceOutputsToolStripMenuItem.Text = "Test Device Outputs"
        '
        'AvatarToolStripMenuItem
        '
        Me.AvatarToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.LoadAvatarToolStripMenuItem, Me.RotateToolStripMenuItem})
        Me.AvatarToolStripMenuItem.Name = "AvatarToolStripMenuItem"
        Me.AvatarToolStripMenuItem.Size = New System.Drawing.Size(53, 20)
        Me.AvatarToolStripMenuItem.Text = "Avatar"
        '
        'LoadAvatarToolStripMenuItem
        '
        Me.LoadAvatarToolStripMenuItem.Name = "LoadAvatarToolStripMenuItem"
        Me.LoadAvatarToolStripMenuItem.Size = New System.Drawing.Size(137, 22)
        Me.LoadAvatarToolStripMenuItem.Text = "Load Avatar"
        '
        'RotateToolStripMenuItem
        '
        Me.RotateToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.X90ToolStripMenuItem, Me.X90ToolStripMenuItem1, Me.Y90ToolStripMenuItem, Me.Y90ToolStripMenuItem1, Me.Z90ToolStripMenuItem, Me.Z90ToolStripMenuItem1})
        Me.RotateToolStripMenuItem.Name = "RotateToolStripMenuItem"
        Me.RotateToolStripMenuItem.Size = New System.Drawing.Size(137, 22)
        Me.RotateToolStripMenuItem.Text = "Rotate"
        '
        'X90ToolStripMenuItem
        '
        Me.X90ToolStripMenuItem.Name = "X90ToolStripMenuItem"
        Me.X90ToolStripMenuItem.Size = New System.Drawing.Size(104, 22)
        Me.X90ToolStripMenuItem.Text = "X 90+"
        '
        'X90ToolStripMenuItem1
        '
        Me.X90ToolStripMenuItem1.Name = "X90ToolStripMenuItem1"
        Me.X90ToolStripMenuItem1.Size = New System.Drawing.Size(104, 22)
        Me.X90ToolStripMenuItem1.Text = "X 90-"
        '
        'Y90ToolStripMenuItem
        '
        Me.Y90ToolStripMenuItem.Name = "Y90ToolStripMenuItem"
        Me.Y90ToolStripMenuItem.Size = New System.Drawing.Size(104, 22)
        Me.Y90ToolStripMenuItem.Text = "Y 90+"
        '
        'Y90ToolStripMenuItem1
        '
        Me.Y90ToolStripMenuItem1.Name = "Y90ToolStripMenuItem1"
        Me.Y90ToolStripMenuItem1.Size = New System.Drawing.Size(104, 22)
        Me.Y90ToolStripMenuItem1.Text = "Y 90-"
        '
        'Z90ToolStripMenuItem
        '
        Me.Z90ToolStripMenuItem.Name = "Z90ToolStripMenuItem"
        Me.Z90ToolStripMenuItem.Size = New System.Drawing.Size(104, 22)
        Me.Z90ToolStripMenuItem.Text = "Z 90+"
        '
        'Z90ToolStripMenuItem1
        '
        Me.Z90ToolStripMenuItem1.Name = "Z90ToolStripMenuItem1"
        Me.Z90ToolStripMenuItem1.Size = New System.Drawing.Size(104, 22)
        Me.Z90ToolStripMenuItem1.Text = "Z 90-"
        '
        'DRenderToolStripMenuItem
        '
        Me.DRenderToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.KeepAtFrontToolStripMenuItem, Me.SpinToolStripMenuItem})
        Me.DRenderToolStripMenuItem.Name = "DRenderToolStripMenuItem"
        Me.DRenderToolStripMenuItem.Size = New System.Drawing.Size(73, 20)
        Me.DRenderToolStripMenuItem.Text = "3D Render"
        '
        'KeepAtFrontToolStripMenuItem
        '
        Me.KeepAtFrontToolStripMenuItem.Enabled = False
        Me.KeepAtFrontToolStripMenuItem.Name = "KeepAtFrontToolStripMenuItem"
        Me.KeepAtFrontToolStripMenuItem.Size = New System.Drawing.Size(187, 22)
        Me.KeepAtFrontToolStripMenuItem.Text = "Keep looking at Front"
        '
        'SpinToolStripMenuItem
        '
        Me.SpinToolStripMenuItem.Name = "SpinToolStripMenuItem"
        Me.SpinToolStripMenuItem.Size = New System.Drawing.Size(187, 22)
        Me.SpinToolStripMenuItem.Text = "Spin"
        '
        'OFDAvatar
        '
        Me.OFDAvatar.Filter = "STL files|*.stl"
        '
        'Button1
        '
        Me.Button1.Enabled = False
        Me.Button1.Location = New System.Drawing.Point(131, 53)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(84, 38)
        Me.Button1.TabIndex = 57
        Me.Button1.Text = "White List"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Enabled = False
        Me.Button2.Location = New System.Drawing.Point(221, 53)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(84, 38)
        Me.Button2.TabIndex = 58
        Me.Button2.Text = "Avatars"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'OFDSettings
        '
        Me.OFDSettings.Filter = "VRHaptics File|*.vrh"
        '
        'SFDSettings
        '
        Me.SFDSettings.Filter = "VRHaptics File|*.vrh"
        '
        'Output
        '
        Me.Output.HeaderText = "#"
        Me.Output.Name = "Output"
        Me.Output.ReadOnly = True
        Me.Output.Width = 20
        '
        'RootBone
        '
        Me.RootBone.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.RootBone.HeaderText = "Root Bone"
        Me.RootBone.Items.AddRange(New Object() {"Unassigned", "Head", "Hips", "Chest", "Right Upper Arm", "Left Upper Arm", "Right Lower Arm", "Left Lower Arm", "Right Hand", "Left Hand", "Right Upper Leg", "Left Upper Leg", "Right Lower Leg", "Left Lower Leg"})
        Me.RootBone.Name = "RootBone"
        Me.RootBone.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.RootBone.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'X
        '
        Me.X.HeaderText = "X"
        Me.X.Name = "X"
        Me.X.Width = 35
        '
        'Y
        '
        Me.Y.HeaderText = "Y"
        Me.Y.Name = "Y"
        Me.Y.Width = 35
        '
        'Z
        '
        Me.Z.HeaderText = "Z"
        Me.Z.Name = "Z"
        Me.Z.Width = 35
        '
        'Activation
        '
        Me.Activation.HeaderText = "D"
        Me.Activation.Name = "Activation"
        Me.Activation.Width = 35
        '
        'Force
        '
        Me.Force.HeaderText = "Force"
        Me.Force.Name = "Force"
        Me.Force.Width = 35
        '
        'Test
        '
        Me.Test.HeaderText = "Test"
        Me.Test.Name = "Test"
        Me.Test.Text = "Test"
        Me.Test.Width = 35
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(755, 529)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.LblLogStatus)
        Me.Controls.Add(Me.GlControl1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "Form1"
        Me.Text = "VRChat Haptics"
        CType(Me.DGVNodes, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        CType(Me.DGVDevice, System.ComponentModel.ISupportInitialize).EndInit()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents MainTimer As Timer
    Friend WithEvents GlControl1 As OpenTK.GLControl
    Friend WithEvents LblLogStatus As Label
    Friend WithEvents RetryTimer As Timer
    Friend WithEvents DGVNodes As DataGridView
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents DGVDevice As DataGridView
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents LoadDeviceNodeDescriptionToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SaveDeviceNodeDescriptionToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DeviceToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AddDeviceToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents EditDeviceToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RemoveDeviceToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents OFDAvatar As OpenFileDialog
    Friend WithEvents AvatarToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents LoadAvatarToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RotateToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents X90ToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents X90ToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents Y90ToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Y90ToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents Z90ToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Z90ToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents OFDSettings As OpenFileDialog
    Friend WithEvents SFDSettings As SaveFileDialog
    Friend WithEvents CloseToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DRenderToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents KeepAtFrontToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SpinToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SetWifiOnDeviceToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents TestDeviceOutputsToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Output As DataGridViewTextBoxColumn
    Friend WithEvents RootBone As DataGridViewComboBoxColumn
    Friend WithEvents X As DataGridViewTextBoxColumn
    Friend WithEvents Y As DataGridViewTextBoxColumn
    Friend WithEvents Z As DataGridViewTextBoxColumn
    Friend WithEvents Activation As DataGridViewTextBoxColumn
    Friend WithEvents Force As DataGridViewTextBoxColumn
    Friend WithEvents Test As DataGridViewButtonColumn
End Class
