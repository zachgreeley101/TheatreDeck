namespace theatredeck.app.forms
{
    partial class TheatreDeckForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            menuStrip1 = new MenuStrip();
            optionsToolStripMenuItem = new ToolStripMenuItem();
            oBSToolStripMenuItem3 = new ToolStripMenuItem();
            quickActionsToolStripMenuItem = new ToolStripMenuItem();
            autoLaunchToolStripMenuItem = new ToolStripMenuItem();
            closeBothToolStripMenuItem1 = new ToolStripMenuItem();
            launchScreenToolStripMenuItem = new ToolStripMenuItem();
            launchScreenToolStripMenuItem1 = new ToolStripMenuItem();
            cameraControlsToolStripMenuItem = new ToolStripMenuItem();
            launchCameraToolStripMenuItem = new ToolStripMenuItem();
            vLCToolStripMenuItem = new ToolStripMenuItem();
            launchVLCToolStripMenuItem = new ToolStripMenuItem();
            exitVLCToolStripMenuItem = new ToolStripMenuItem();
            restartVLCToolStripMenuItem = new ToolStripMenuItem();
            testToolStripMenuItem = new ToolStripMenuItem();
            testButton1ToolStripMenuItem = new ToolStripMenuItem();
            testButton2ToolStripMenuItem = new ToolStripMenuItem();
            notionFunctionsToolStripMenuItem = new ToolStripMenuItem();
            queryDatabaseSchemaToolStripMenuItem = new ToolStripMenuItem();
            queryDatabaseByFilterToolStripMenuItem = new ToolStripMenuItem();
            updatePagePropertyToolStripMenuItem = new ToolStripMenuItem();
            createNewPageToolStripMenuItem = new ToolStripMenuItem();
            placefeatToolStripMenuItem = new ToolStripMenuItem();
            TimerAutoRefreshLog = new System.Windows.Forms.Timer(components);
            tabPageLog = new TabPage();
            tabPageMain = new TabPage();
            tabControlSubManager = new TabControl();
            tabPageManagerMedia = new TabPage();
            groupBox2 = new GroupBox();
            labCurrentTime = new Label();
            labTotalLength = new Label();
            trackBarVolume = new TrackBar();
            labPlaybackState = new Label();
            lblNowPlaying = new Label();
            lblVolumePercent = new Label();
            groupBox1 = new GroupBox();
            labVolMediaTag = new Label();
            labEndMediaTag = new Label();
            labStartMediaTag = new Label();
            btnSetVol = new Button();
            btnSetEnd = new Button();
            btnSetStart = new Button();
            lstPlaylist = new ListBox();
            btnMoveDown = new Button();
            btnMoveUp = new Button();
            btnRemoveFromPlaylist = new Button();
            bntBrowseAdd = new Button();
            btnPause = new Button();
            btnRefreshStatus = new Button();
            labFilename = new Label();
            tabPageManagerOBS = new TabPage();
            checkBoxLavaLamps = new CheckBox();
            BtnObsTest = new Button();
            tabControlTopLevel = new TabControl();
            tabPageSettings = new TabPage();
            label5 = new Label();
            comboBoxOperationMode = new ComboBox();
            tabPageScrape = new TabPage();
            tabControlScrap = new TabControl();
            tabPageScrapeLocal = new TabPage();
            labScraperLocal = new Label();
            progressBarScraperLocal = new ProgressBar();
            btnScrapeMD6 = new Button();
            btnScrapeMD5 = new Button();
            btnScrapeMD4 = new Button();
            btnScrapeMD3 = new Button();
            btnScrapeMD2 = new Button();
            btnScrapeMD1 = new Button();
            tabPageScrapeWeb = new TabPage();
            btnScrapeAuto = new Button();
            menuStrip1.SuspendLayout();
            tabPageMain.SuspendLayout();
            tabControlSubManager.SuspendLayout();
            tabPageManagerMedia.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarVolume).BeginInit();
            groupBox1.SuspendLayout();
            tabPageManagerOBS.SuspendLayout();
            tabControlTopLevel.SuspendLayout();
            tabPageSettings.SuspendLayout();
            tabPageScrape.SuspendLayout();
            tabControlScrap.SuspendLayout();
            tabPageScrapeLocal.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { optionsToolStripMenuItem, testToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // optionsToolStripMenuItem
            // 
            optionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { oBSToolStripMenuItem3, vLCToolStripMenuItem });
            optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            optionsToolStripMenuItem.Size = new Size(47, 20);
            optionsToolStripMenuItem.Text = "Tools";
            // 
            // oBSToolStripMenuItem3
            // 
            oBSToolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[] { quickActionsToolStripMenuItem, launchScreenToolStripMenuItem, cameraControlsToolStripMenuItem });
            oBSToolStripMenuItem3.Name = "oBSToolStripMenuItem3";
            oBSToolStripMenuItem3.Size = new Size(144, 22);
            oBSToolStripMenuItem3.Text = "OBS Controls";
            // 
            // quickActionsToolStripMenuItem
            // 
            quickActionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { autoLaunchToolStripMenuItem, closeBothToolStripMenuItem1 });
            quickActionsToolStripMenuItem.Name = "quickActionsToolStripMenuItem";
            quickActionsToolStripMenuItem.Size = new Size(158, 22);
            quickActionsToolStripMenuItem.Text = "Quick Actions";
            // 
            // autoLaunchToolStripMenuItem
            // 
            autoLaunchToolStripMenuItem.Name = "autoLaunchToolStripMenuItem";
            autoLaunchToolStripMenuItem.Size = new Size(141, 22);
            autoLaunchToolStripMenuItem.Text = "Launch Both";
            autoLaunchToolStripMenuItem.Click += autoLaunchToolStripMenuItem_Click;
            // 
            // closeBothToolStripMenuItem1
            // 
            closeBothToolStripMenuItem1.Name = "closeBothToolStripMenuItem1";
            closeBothToolStripMenuItem1.Size = new Size(141, 22);
            closeBothToolStripMenuItem1.Text = "Close Both";
            // 
            // launchScreenToolStripMenuItem
            // 
            launchScreenToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { launchScreenToolStripMenuItem1 });
            launchScreenToolStripMenuItem.Name = "launchScreenToolStripMenuItem";
            launchScreenToolStripMenuItem.Size = new Size(158, 22);
            launchScreenToolStripMenuItem.Text = "Screen Actions";
            // 
            // launchScreenToolStripMenuItem1
            // 
            launchScreenToolStripMenuItem1.Name = "launchScreenToolStripMenuItem1";
            launchScreenToolStripMenuItem1.Size = new Size(151, 22);
            launchScreenToolStripMenuItem1.Text = "Launch Screen";
            launchScreenToolStripMenuItem1.Click += launchScreenToolStripMenuItem1_Click;
            // 
            // cameraControlsToolStripMenuItem
            // 
            cameraControlsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { launchCameraToolStripMenuItem });
            cameraControlsToolStripMenuItem.Name = "cameraControlsToolStripMenuItem";
            cameraControlsToolStripMenuItem.Size = new Size(158, 22);
            cameraControlsToolStripMenuItem.Text = "Camera Actions";
            // 
            // launchCameraToolStripMenuItem
            // 
            launchCameraToolStripMenuItem.Name = "launchCameraToolStripMenuItem";
            launchCameraToolStripMenuItem.Size = new Size(157, 22);
            launchCameraToolStripMenuItem.Text = "Launch Camera";
            launchCameraToolStripMenuItem.Click += launchCameraToolStripMenuItem_Click;
            // 
            // vLCToolStripMenuItem
            // 
            vLCToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { launchVLCToolStripMenuItem, exitVLCToolStripMenuItem, restartVLCToolStripMenuItem });
            vLCToolStripMenuItem.Name = "vLCToolStripMenuItem";
            vLCToolStripMenuItem.Size = new Size(144, 22);
            vLCToolStripMenuItem.Text = "VLC";
            // 
            // launchVLCToolStripMenuItem
            // 
            launchVLCToolStripMenuItem.Name = "launchVLCToolStripMenuItem";
            launchVLCToolStripMenuItem.Size = new Size(137, 22);
            launchVLCToolStripMenuItem.Text = "Launch VLC";
            launchVLCToolStripMenuItem.Click += launchVLCToolStripMenuItem_Click;
            // 
            // exitVLCToolStripMenuItem
            // 
            exitVLCToolStripMenuItem.Name = "exitVLCToolStripMenuItem";
            exitVLCToolStripMenuItem.Size = new Size(137, 22);
            exitVLCToolStripMenuItem.Text = "Exit VLC";
            exitVLCToolStripMenuItem.Click += exitVLCToolStripMenuItem_Click;
            // 
            // restartVLCToolStripMenuItem
            // 
            restartVLCToolStripMenuItem.Name = "restartVLCToolStripMenuItem";
            restartVLCToolStripMenuItem.Size = new Size(137, 22);
            restartVLCToolStripMenuItem.Text = "Restart VLC";
            restartVLCToolStripMenuItem.Click += restartVLCToolStripMenuItem_Click;
            // 
            // testToolStripMenuItem
            // 
            testToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { testButton1ToolStripMenuItem, testButton2ToolStripMenuItem, notionFunctionsToolStripMenuItem });
            testToolStripMenuItem.Name = "testToolStripMenuItem";
            testToolStripMenuItem.Size = new Size(40, 20);
            testToolStripMenuItem.Text = "Test";
            // 
            // testButton1ToolStripMenuItem
            // 
            testButton1ToolStripMenuItem.Name = "testButton1ToolStripMenuItem";
            testButton1ToolStripMenuItem.Size = new Size(166, 22);
            testButton1ToolStripMenuItem.Text = "Test Button 1";
            testButton1ToolStripMenuItem.Click += testButton1ToolStripMenuItem_Click;
            // 
            // testButton2ToolStripMenuItem
            // 
            testButton2ToolStripMenuItem.Name = "testButton2ToolStripMenuItem";
            testButton2ToolStripMenuItem.Size = new Size(166, 22);
            testButton2ToolStripMenuItem.Text = "Test Button 2";
            testButton2ToolStripMenuItem.Click += testButton2ToolStripMenuItem_Click;
            // 
            // notionFunctionsToolStripMenuItem
            // 
            notionFunctionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { queryDatabaseSchemaToolStripMenuItem, queryDatabaseByFilterToolStripMenuItem, updatePagePropertyToolStripMenuItem, createNewPageToolStripMenuItem });
            notionFunctionsToolStripMenuItem.Name = "notionFunctionsToolStripMenuItem";
            notionFunctionsToolStripMenuItem.Size = new Size(166, 22);
            notionFunctionsToolStripMenuItem.Text = "Notion Functions";
            // 
            // queryDatabaseSchemaToolStripMenuItem
            // 
            queryDatabaseSchemaToolStripMenuItem.Name = "queryDatabaseSchemaToolStripMenuItem";
            queryDatabaseSchemaToolStripMenuItem.Size = new Size(202, 22);
            queryDatabaseSchemaToolStripMenuItem.Text = "Query Database Schema";
            queryDatabaseSchemaToolStripMenuItem.Click += queryDatabaseSchemaToolStripMenuItem_Click;
            // 
            // queryDatabaseByFilterToolStripMenuItem
            // 
            queryDatabaseByFilterToolStripMenuItem.Name = "queryDatabaseByFilterToolStripMenuItem";
            queryDatabaseByFilterToolStripMenuItem.Size = new Size(202, 22);
            queryDatabaseByFilterToolStripMenuItem.Text = "Query Database By Filter";
            queryDatabaseByFilterToolStripMenuItem.Click += queryDatabaseByFilterToolStripMenuItem_Click;
            // 
            // updatePagePropertyToolStripMenuItem
            // 
            updatePagePropertyToolStripMenuItem.Name = "updatePagePropertyToolStripMenuItem";
            updatePagePropertyToolStripMenuItem.Size = new Size(202, 22);
            updatePagePropertyToolStripMenuItem.Text = "Update Page Property";
            updatePagePropertyToolStripMenuItem.Click += updatePagePropertyToolStripMenuItem_Click;
            // 
            // createNewPageToolStripMenuItem
            // 
            createNewPageToolStripMenuItem.Name = "createNewPageToolStripMenuItem";
            createNewPageToolStripMenuItem.Size = new Size(202, 22);
            createNewPageToolStripMenuItem.Text = "Create New Page";
            createNewPageToolStripMenuItem.Click += createNewPageToolStripMenuItem_Click;
            // 
            // placefeatToolStripMenuItem
            // 
            placefeatToolStripMenuItem.Name = "placefeatToolStripMenuItem";
            placefeatToolStripMenuItem.Size = new Size(137, 22);
            placefeatToolStripMenuItem.Text = "Insert (feat).";
            // 
            // TimerAutoRefreshLog
            // 
            TimerAutoRefreshLog.Enabled = true;
            TimerAutoRefreshLog.Interval = 1000;
            // 
            // tabPageLog
            // 
            tabPageLog.Location = new Point(4, 24);
            tabPageLog.Name = "tabPageLog";
            tabPageLog.Size = new Size(792, 393);
            tabPageLog.TabIndex = 3;
            tabPageLog.Text = "Logs";
            tabPageLog.UseVisualStyleBackColor = true;
            // 
            // tabPageMain
            // 
            tabPageMain.Controls.Add(tabControlSubManager);
            tabPageMain.Location = new Point(4, 24);
            tabPageMain.Name = "tabPageMain";
            tabPageMain.Size = new Size(792, 393);
            tabPageMain.TabIndex = 2;
            tabPageMain.Text = "Main";
            tabPageMain.UseVisualStyleBackColor = true;
            // 
            // tabControlSubManager
            // 
            tabControlSubManager.Controls.Add(tabPageManagerMedia);
            tabControlSubManager.Controls.Add(tabPageManagerOBS);
            tabControlSubManager.Dock = DockStyle.Fill;
            tabControlSubManager.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tabControlSubManager.Location = new Point(0, 0);
            tabControlSubManager.Multiline = true;
            tabControlSubManager.Name = "tabControlSubManager";
            tabControlSubManager.SelectedIndex = 0;
            tabControlSubManager.Size = new Size(792, 393);
            tabControlSubManager.SizeMode = TabSizeMode.Fixed;
            tabControlSubManager.TabIndex = 0;
            // 
            // tabPageManagerMedia
            // 
            tabPageManagerMedia.BackColor = Color.Transparent;
            tabPageManagerMedia.Controls.Add(groupBox2);
            tabPageManagerMedia.Controls.Add(groupBox1);
            tabPageManagerMedia.Controls.Add(lstPlaylist);
            tabPageManagerMedia.Controls.Add(btnMoveDown);
            tabPageManagerMedia.Controls.Add(btnMoveUp);
            tabPageManagerMedia.Controls.Add(btnRemoveFromPlaylist);
            tabPageManagerMedia.Controls.Add(bntBrowseAdd);
            tabPageManagerMedia.Controls.Add(btnPause);
            tabPageManagerMedia.Controls.Add(btnRefreshStatus);
            tabPageManagerMedia.Controls.Add(labFilename);
            tabPageManagerMedia.Location = new Point(4, 24);
            tabPageManagerMedia.Name = "tabPageManagerMedia";
            tabPageManagerMedia.Padding = new Padding(3);
            tabPageManagerMedia.Size = new Size(784, 365);
            tabPageManagerMedia.TabIndex = 0;
            tabPageManagerMedia.Text = "Media";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(labCurrentTime);
            groupBox2.Controls.Add(labTotalLength);
            groupBox2.Controls.Add(trackBarVolume);
            groupBox2.Controls.Add(labPlaybackState);
            groupBox2.Controls.Add(lblNowPlaying);
            groupBox2.Controls.Add(lblVolumePercent);
            groupBox2.Location = new Point(3, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(411, 199);
            groupBox2.TabIndex = 17;
            groupBox2.TabStop = false;
            groupBox2.Text = "Current Media";
            // 
            // labCurrentTime
            // 
            labCurrentTime.AutoSize = true;
            labCurrentTime.Location = new Point(28, 30);
            labCurrentTime.Name = "labCurrentTime";
            labCurrentTime.Size = new Size(74, 15);
            labCurrentTime.TabIndex = 0;
            labCurrentTime.Text = "CurrentTime";
            // 
            // labTotalLength
            // 
            labTotalLength.AutoSize = true;
            labTotalLength.Location = new Point(28, 52);
            labTotalLength.Name = "labTotalLength";
            labTotalLength.Size = new Size(70, 15);
            labTotalLength.TabIndex = 1;
            labTotalLength.Text = "TotalLength";
            // 
            // trackBarVolume
            // 
            trackBarVolume.LargeChange = 10;
            trackBarVolume.Location = new Point(360, 15);
            trackBarVolume.Maximum = 200;
            trackBarVolume.Name = "trackBarVolume";
            trackBarVolume.Orientation = Orientation.Vertical;
            trackBarVolume.Size = new Size(45, 159);
            trackBarVolume.TabIndex = 15;
            trackBarVolume.TickFrequency = 5;
            trackBarVolume.Value = 100;
            trackBarVolume.Scroll += trackBarVolume_Scroll;
            // 
            // labPlaybackState
            // 
            labPlaybackState.AutoSize = true;
            labPlaybackState.Location = new Point(6, 143);
            labPlaybackState.Name = "labPlaybackState";
            labPlaybackState.Size = new Size(83, 15);
            labPlaybackState.TabIndex = 3;
            labPlaybackState.Text = "Playback State";
            // 
            // lblNowPlaying
            // 
            lblNowPlaying.AutoSize = true;
            lblNowPlaying.Location = new Point(6, 166);
            lblNowPlaying.Name = "lblNowPlaying";
            lblNowPlaying.Size = new Size(74, 15);
            lblNowPlaying.TabIndex = 14;
            lblNowPlaying.Text = "Now Playing";
            // 
            // lblVolumePercent
            // 
            lblVolumePercent.AutoSize = true;
            lblVolumePercent.Location = new Point(360, 174);
            lblVolumePercent.Name = "lblVolumePercent";
            lblVolumePercent.Size = new Size(35, 15);
            lblVolumePercent.TabIndex = 2;
            lblVolumePercent.Text = "100%";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(labVolMediaTag);
            groupBox1.Controls.Add(labEndMediaTag);
            groupBox1.Controls.Add(labStartMediaTag);
            groupBox1.Controls.Add(btnSetVol);
            groupBox1.Controls.Add(btnSetEnd);
            groupBox1.Controls.Add(btnSetStart);
            groupBox1.Location = new Point(494, 27);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(248, 124);
            groupBox1.TabIndex = 16;
            groupBox1.TabStop = false;
            groupBox1.Text = "Current Media Tags";
            // 
            // labVolMediaTag
            // 
            labVolMediaTag.AutoSize = true;
            labVolMediaTag.Location = new Point(9, 91);
            labVolMediaTag.Name = "labVolMediaTag";
            labVolMediaTag.Size = new Size(67, 15);
            labVolMediaTag.TabIndex = 21;
            labVolMediaTag.Text = "Vol-UnsetO";
            // 
            // labEndMediaTag
            // 
            labEndMediaTag.AutoSize = true;
            labEndMediaTag.Location = new Point(8, 61);
            labEndMediaTag.Name = "labEndMediaTag";
            labEndMediaTag.Size = new Size(71, 15);
            labEndMediaTag.TabIndex = 20;
            labEndMediaTag.Text = "End-UnsetO";
            // 
            // labStartMediaTag
            // 
            labStartMediaTag.AutoSize = true;
            labStartMediaTag.Location = new Point(8, 32);
            labStartMediaTag.Name = "labStartMediaTag";
            labStartMediaTag.Size = new Size(75, 15);
            labStartMediaTag.TabIndex = 17;
            labStartMediaTag.Text = "Start-UnsetO";
            // 
            // btnSetVol
            // 
            btnSetVol.Location = new Point(167, 87);
            btnSetVol.Name = "btnSetVol";
            btnSetVol.Size = new Size(75, 23);
            btnSetVol.TabIndex = 19;
            btnSetVol.Text = "Set Volume";
            btnSetVol.UseVisualStyleBackColor = true;
            btnSetVol.Click += btnSetVol_Click;
            // 
            // btnSetEnd
            // 
            btnSetEnd.Location = new Point(167, 58);
            btnSetEnd.Name = "btnSetEnd";
            btnSetEnd.Size = new Size(75, 23);
            btnSetEnd.TabIndex = 18;
            btnSetEnd.Text = "Set End";
            btnSetEnd.UseVisualStyleBackColor = true;
            btnSetEnd.Click += btnSetEnd_Click;
            // 
            // btnSetStart
            // 
            btnSetStart.Location = new Point(167, 28);
            btnSetStart.Name = "btnSetStart";
            btnSetStart.Size = new Size(75, 23);
            btnSetStart.TabIndex = 17;
            btnSetStart.Text = "Set Start";
            btnSetStart.UseVisualStyleBackColor = true;
            btnSetStart.Click += btnSetStart_Click;
            // 
            // lstPlaylist
            // 
            lstPlaylist.FormattingEnabled = true;
            lstPlaylist.ItemHeight = 15;
            lstPlaylist.Location = new Point(6, 259);
            lstPlaylist.Name = "lstPlaylist";
            lstPlaylist.Size = new Size(772, 94);
            lstPlaylist.TabIndex = 13;
            lstPlaylist.DoubleClick += lstPlaylist_DoubleClick;
            // 
            // btnMoveDown
            // 
            btnMoveDown.Location = new Point(680, 230);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(98, 23);
            btnMoveDown.TabIndex = 12;
            btnMoveDown.Text = "Move Down";
            btnMoveDown.UseVisualStyleBackColor = true;
            btnMoveDown.Click += btnMoveDown_Click;
            // 
            // btnMoveUp
            // 
            btnMoveUp.Location = new Point(576, 230);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(98, 23);
            btnMoveUp.TabIndex = 11;
            btnMoveUp.Text = "Move Up";
            btnMoveUp.UseVisualStyleBackColor = true;
            btnMoveUp.Click += btnMoveUp_Click;
            // 
            // btnRemoveFromPlaylist
            // 
            btnRemoveFromPlaylist.Location = new Point(472, 230);
            btnRemoveFromPlaylist.Name = "btnRemoveFromPlaylist";
            btnRemoveFromPlaylist.Size = new Size(98, 23);
            btnRemoveFromPlaylist.TabIndex = 10;
            btnRemoveFromPlaylist.Text = "Remove";
            btnRemoveFromPlaylist.UseVisualStyleBackColor = true;
            btnRemoveFromPlaylist.Click += btnRemoveFromPlaylist_Click;
            // 
            // bntBrowseAdd
            // 
            bntBrowseAdd.Location = new Point(7, 230);
            bntBrowseAdd.Name = "bntBrowseAdd";
            bntBrowseAdd.Size = new Size(98, 23);
            bntBrowseAdd.TabIndex = 9;
            bntBrowseAdd.Text = "Browse";
            bntBrowseAdd.UseVisualStyleBackColor = true;
            bntBrowseAdd.Click += bntBrowseAdd_Click;
            // 
            // btnPause
            // 
            btnPause.Location = new Point(111, 230);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(98, 23);
            btnPause.TabIndex = 7;
            btnPause.Text = "Pause";
            btnPause.UseVisualStyleBackColor = true;
            btnPause.Click += btnPause_Click;
            // 
            // btnRefreshStatus
            // 
            btnRefreshStatus.Location = new Point(420, 179);
            btnRefreshStatus.Name = "btnRefreshStatus";
            btnRefreshStatus.Size = new Size(98, 23);
            btnRefreshStatus.TabIndex = 5;
            btnRefreshStatus.Text = "Refresh Status";
            btnRefreshStatus.UseVisualStyleBackColor = true;
            btnRefreshStatus.Click += btnRefreshStatus_Click;
            // 
            // labFilename
            // 
            labFilename.AutoSize = true;
            labFilename.Location = new Point(6, 205);
            labFilename.Name = "labFilename";
            labFilename.Size = new Size(55, 15);
            labFilename.TabIndex = 4;
            labFilename.Text = "Filename";
            // 
            // tabPageManagerOBS
            // 
            tabPageManagerOBS.Controls.Add(checkBoxLavaLamps);
            tabPageManagerOBS.Controls.Add(BtnObsTest);
            tabPageManagerOBS.Location = new Point(4, 24);
            tabPageManagerOBS.Name = "tabPageManagerOBS";
            tabPageManagerOBS.Padding = new Padding(3);
            tabPageManagerOBS.Size = new Size(784, 365);
            tabPageManagerOBS.TabIndex = 1;
            tabPageManagerOBS.Text = "OBS";
            tabPageManagerOBS.UseVisualStyleBackColor = true;
            // 
            // checkBoxLavaLamps
            // 
            checkBoxLavaLamps.AutoSize = true;
            checkBoxLavaLamps.Location = new Point(530, 43);
            checkBoxLavaLamps.Name = "checkBoxLavaLamps";
            checkBoxLavaLamps.Size = new Size(88, 19);
            checkBoxLavaLamps.TabIndex = 3;
            checkBoxLavaLamps.Text = "Lava Lamps";
            checkBoxLavaLamps.UseVisualStyleBackColor = true;
            checkBoxLavaLamps.CheckedChanged += checkBoxLavaLamps_CheckedChanged;
            // 
            // BtnObsTest
            // 
            BtnObsTest.Location = new Point(188, 81);
            BtnObsTest.Name = "BtnObsTest";
            BtnObsTest.Size = new Size(103, 23);
            BtnObsTest.TabIndex = 2;
            BtnObsTest.Text = "OBS Test";
            BtnObsTest.UseVisualStyleBackColor = true;
            BtnObsTest.Click += BtnObsTest_Click;
            // 
            // tabControlTopLevel
            // 
            tabControlTopLevel.Controls.Add(tabPageMain);
            tabControlTopLevel.Controls.Add(tabPageSettings);
            tabControlTopLevel.Controls.Add(tabPageScrape);
            tabControlTopLevel.Controls.Add(tabPageLog);
            tabControlTopLevel.Location = new Point(0, 27);
            tabControlTopLevel.Name = "tabControlTopLevel";
            tabControlTopLevel.SelectedIndex = 0;
            tabControlTopLevel.Size = new Size(800, 421);
            tabControlTopLevel.TabIndex = 1;
            // 
            // tabPageSettings
            // 
            tabPageSettings.Controls.Add(label5);
            tabPageSettings.Controls.Add(comboBoxOperationMode);
            tabPageSettings.Location = new Point(4, 24);
            tabPageSettings.Name = "tabPageSettings";
            tabPageSettings.Padding = new Padding(3);
            tabPageSettings.Size = new Size(792, 393);
            tabPageSettings.TabIndex = 4;
            tabPageSettings.Text = "Settings";
            tabPageSettings.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(8, 14);
            label5.Name = "label5";
            label5.Size = new Size(94, 15);
            label5.TabIndex = 2;
            label5.Text = "Operation Mode";
            // 
            // comboBoxOperationMode
            // 
            comboBoxOperationMode.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOperationMode.FormattingEnabled = true;
            comboBoxOperationMode.Location = new Point(8, 32);
            comboBoxOperationMode.Name = "comboBoxOperationMode";
            comboBoxOperationMode.Size = new Size(121, 23);
            comboBoxOperationMode.TabIndex = 1;
            comboBoxOperationMode.SelectedIndexChanged += comboBoxOperationMode_SelectedIndexChanged;
            // 
            // tabPageScrape
            // 
            tabPageScrape.Controls.Add(tabControlScrap);
            tabPageScrape.Location = new Point(4, 24);
            tabPageScrape.Name = "tabPageScrape";
            tabPageScrape.Padding = new Padding(3);
            tabPageScrape.Size = new Size(792, 393);
            tabPageScrape.TabIndex = 5;
            tabPageScrape.Text = "Scraper";
            tabPageScrape.UseVisualStyleBackColor = true;
            // 
            // tabControlScrap
            // 
            tabControlScrap.Controls.Add(tabPageScrapeLocal);
            tabControlScrap.Controls.Add(tabPageScrapeWeb);
            tabControlScrap.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tabControlScrap.ItemSize = new Size(96, 20);
            tabControlScrap.Location = new Point(0, 0);
            tabControlScrap.Name = "tabControlScrap";
            tabControlScrap.SelectedIndex = 0;
            tabControlScrap.Size = new Size(789, 387);
            tabControlScrap.TabIndex = 0;
            // 
            // tabPageScrapeLocal
            // 
            tabPageScrapeLocal.Controls.Add(btnScrapeAuto);
            tabPageScrapeLocal.Controls.Add(labScraperLocal);
            tabPageScrapeLocal.Controls.Add(progressBarScraperLocal);
            tabPageScrapeLocal.Controls.Add(btnScrapeMD6);
            tabPageScrapeLocal.Controls.Add(btnScrapeMD5);
            tabPageScrapeLocal.Controls.Add(btnScrapeMD4);
            tabPageScrapeLocal.Controls.Add(btnScrapeMD3);
            tabPageScrapeLocal.Controls.Add(btnScrapeMD2);
            tabPageScrapeLocal.Controls.Add(btnScrapeMD1);
            tabPageScrapeLocal.Location = new Point(4, 24);
            tabPageScrapeLocal.Name = "tabPageScrapeLocal";
            tabPageScrapeLocal.Padding = new Padding(3);
            tabPageScrapeLocal.Size = new Size(781, 359);
            tabPageScrapeLocal.TabIndex = 0;
            tabPageScrapeLocal.Text = "Local ";
            tabPageScrapeLocal.UseVisualStyleBackColor = true;
            // 
            // labScraperLocal
            // 
            labScraperLocal.AutoSize = true;
            labScraperLocal.Location = new Point(43, 289);
            labScraperLocal.Name = "labScraperLocal";
            labScraperLocal.Size = new Size(101, 15);
            labScraperLocal.TabIndex = 7;
            labScraperLocal.Text = "// Scraping Status";
            // 
            // progressBarScraperLocal
            // 
            progressBarScraperLocal.Location = new Point(43, 307);
            progressBarScraperLocal.Name = "progressBarScraperLocal";
            progressBarScraperLocal.Size = new Size(169, 23);
            progressBarScraperLocal.TabIndex = 6;
            // 
            // btnScrapeMD6
            // 
            btnScrapeMD6.Location = new Point(43, 219);
            btnScrapeMD6.Name = "btnScrapeMD6";
            btnScrapeMD6.Size = new Size(81, 23);
            btnScrapeMD6.TabIndex = 5;
            btnScrapeMD6.Text = "MD 6";
            btnScrapeMD6.UseVisualStyleBackColor = true;
            btnScrapeMD6.Click += btnScrapeMD6_Click;
            // 
            // btnScrapeMD5
            // 
            btnScrapeMD5.Location = new Point(43, 190);
            btnScrapeMD5.Name = "btnScrapeMD5";
            btnScrapeMD5.Size = new Size(81, 23);
            btnScrapeMD5.TabIndex = 4;
            btnScrapeMD5.Text = "MD 5";
            btnScrapeMD5.UseVisualStyleBackColor = true;
            btnScrapeMD5.Click += btnScrapeMD5_Click;
            // 
            // btnScrapeMD4
            // 
            btnScrapeMD4.Location = new Point(43, 161);
            btnScrapeMD4.Name = "btnScrapeMD4";
            btnScrapeMD4.Size = new Size(81, 23);
            btnScrapeMD4.TabIndex = 3;
            btnScrapeMD4.Text = "MD 4";
            btnScrapeMD4.UseVisualStyleBackColor = true;
            btnScrapeMD4.Click += btnScrapeMD4_Click;
            // 
            // btnScrapeMD3
            // 
            btnScrapeMD3.Location = new Point(43, 132);
            btnScrapeMD3.Name = "btnScrapeMD3";
            btnScrapeMD3.Size = new Size(81, 23);
            btnScrapeMD3.TabIndex = 2;
            btnScrapeMD3.Text = "MD 3";
            btnScrapeMD3.UseVisualStyleBackColor = true;
            btnScrapeMD3.Click += btnScrapeMD3_Click;
            // 
            // btnScrapeMD2
            // 
            btnScrapeMD2.Location = new Point(43, 103);
            btnScrapeMD2.Name = "btnScrapeMD2";
            btnScrapeMD2.Size = new Size(81, 23);
            btnScrapeMD2.TabIndex = 1;
            btnScrapeMD2.Text = "MD 2";
            btnScrapeMD2.UseVisualStyleBackColor = true;
            btnScrapeMD2.Click += btnScrapeMD2_Click;
            // 
            // btnScrapeMD1
            // 
            btnScrapeMD1.Location = new Point(43, 74);
            btnScrapeMD1.Name = "btnScrapeMD1";
            btnScrapeMD1.Size = new Size(81, 23);
            btnScrapeMD1.TabIndex = 0;
            btnScrapeMD1.Text = "MD 1";
            btnScrapeMD1.UseVisualStyleBackColor = true;
            btnScrapeMD1.Click += btnScrapeMD1_Click;
            // 
            // tabPageScrapeWeb
            // 
            tabPageScrapeWeb.Location = new Point(4, 24);
            tabPageScrapeWeb.Name = "tabPageScrapeWeb";
            tabPageScrapeWeb.Padding = new Padding(3);
            tabPageScrapeWeb.Size = new Size(781, 359);
            tabPageScrapeWeb.TabIndex = 1;
            tabPageScrapeWeb.Text = "Web";
            tabPageScrapeWeb.UseVisualStyleBackColor = true;
            // 
            // btnScrapeAuto
            // 
            btnScrapeAuto.Location = new Point(43, 45);
            btnScrapeAuto.Name = "btnScrapeAuto";
            btnScrapeAuto.Size = new Size(81, 23);
            btnScrapeAuto.TabIndex = 8;
            btnScrapeAuto.Text = "AUTO";
            btnScrapeAuto.UseVisualStyleBackColor = true;
            btnScrapeAuto.Click += btnScrapeAuto_Click;
            // 
            // TheatreDeckForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tabControlTopLevel);
            Controls.Add(menuStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            Name = "TheatreDeckForm";
            Text = "Control panel";
            TopMost = true;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tabPageMain.ResumeLayout(false);
            tabControlSubManager.ResumeLayout(false);
            tabPageManagerMedia.ResumeLayout(false);
            tabPageManagerMedia.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarVolume).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tabPageManagerOBS.ResumeLayout(false);
            tabPageManagerOBS.PerformLayout();
            tabControlTopLevel.ResumeLayout(false);
            tabPageSettings.ResumeLayout(false);
            tabPageSettings.PerformLayout();
            tabPageScrape.ResumeLayout(false);
            tabControlScrap.ResumeLayout(false);
            tabPageScrapeLocal.ResumeLayout(false);
            tabPageScrapeLocal.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private System.Windows.Forms.Timer TimerAutoRefreshLog;
        private ToolStripMenuItem placefeatToolStripMenuItem;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem oBSToolStripMenuItem3;
        private ToolStripMenuItem launchScreenToolStripMenuItem;
        private ToolStripMenuItem launchScreenToolStripMenuItem1;
        private ToolStripMenuItem cameraControlsToolStripMenuItem;
        private ToolStripMenuItem launchCameraToolStripMenuItem;
        private ToolStripMenuItem quickActionsToolStripMenuItem;
        private ToolStripMenuItem autoLaunchToolStripMenuItem;
        private ToolStripMenuItem closeBothToolStripMenuItem1;
        private ToolStripMenuItem testToolStripMenuItem;
        private ToolStripMenuItem testButton1ToolStripMenuItem;
        private ToolStripMenuItem testButton2ToolStripMenuItem;
        private ToolStripMenuItem notionFunctionsToolStripMenuItem;
        private ToolStripMenuItem queryDatabaseByFilterToolStripMenuItem;
        private ToolStripMenuItem updatePagePropertyToolStripMenuItem;
        private ToolStripMenuItem createNewPageToolStripMenuItem;
        private ToolStripMenuItem queryDatabaseSchemaToolStripMenuItem;
        private TabPage tabPageLog;
        private TabPage tabPageMain;
        private TabControl tabControlSubManager;
        private TabPage tabPageManagerMedia;
        private Button BtnPlayNextTrack;
        private TabPage tabPageManagerOBS;
        private Button BtnObsTest;
        private TabControl tabControlTopLevel;
        private CheckBox checkBoxLavaLamps;
        private TabPage tabPageSettings;
        private ComboBox comboBoxOperationMode;
        private Label label5;
        private ProgressBar volumeMeter;
        private Label labTotalLength;
        private Label labCurrentTime;
        private Label labFilename;
        private Label labPlaybackState;
        private Label lblVolumePercent;
        private Button btnRefreshStatus;
        private Button btnPause;
        private Button bntBrowseAdd;
        private Button btnRemoveFromPlaylist;
        private Button btnMoveDown;
        private Button btnMoveUp;
        private ListBox lstPlaylist;
        private Label lblNowPlaying;
        private TrackBar trackBarVolume;
        private ToolStripMenuItem vLCToolStripMenuItem;
        private ToolStripMenuItem launchVLCToolStripMenuItem;
        private ToolStripMenuItem exitVLCToolStripMenuItem;
        private ToolStripMenuItem restartVLCToolStripMenuItem;
        private TabPage tabPageScrape;
        private TabControl tabControlScrap;
        private TabPage tabPageScrapeLocal;
        private TabPage tabPageScrapeWeb;
        private Button btnScrapeMD3;
        private Button btnScrapeMD2;
        private Button btnScrapeMD1;
        private ProgressBar progressBarScraperLocal;
        private Button btnScrapeMD6;
        private Button btnScrapeMD5;
        private Button btnScrapeMD4;
        private Label labScraperLocal;
        private GroupBox groupBox1;
        private Button btnSetEnd;
        private Button btnSetStart;
        private Button btnSetVol;
        private Label labStartMediaTag;
        private Label labEndMediaTag;
        private Label labVolMediaTag;
        private GroupBox groupBox2;
        private Button btnScrapeAuto;
    }
}
