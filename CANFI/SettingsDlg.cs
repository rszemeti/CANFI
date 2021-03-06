﻿//
// CANFI (C)heap (A)utomatic (N)oise (F)igure (I)ndicator 
// Copyright (C) 2015 DL2ALF
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.using System;
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using CANFICore;

namespace CANFI
{
    public partial class SettingsDlg : Form
    {
        public bool InvalidateCalibration = false;

        private enum COMSTAT
        {
            NONE = 0,
            RTS = 1,
            DTR = 2
        }

        public SettingsDlg()
        {
            InitializeComponent();
            try
            {
                // populate devices combo box
                cbb_Device.Items.Clear();
                cbb_Device.Items.AddRange((RTLDevice[])Properties.Settings.Default.RTL_Devices);
                RTLDevice d = new RTLDevice();
                foreach (RTLDevice item in cbb_Device.Items)
                {
                    if (item.Name == Properties.Settings.Default.RTL_Device)
                        cbb_Device.SelectedItem = item;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error while getting RTL devices");
            }
            try
            {
                cbb_COM.Items.Clear();
                cbb_COM.Items.AddRange(SerialPort.GetPortNames());
                cbb_COM.Items.Insert(0, "[none]");
                cbb_COM.SelectedItem = Properties.Settings.Default.COM_Port;

                cbb_COM_Noise.Items.AddRange(new string[] { COMSTAT.RTS.ToString(), COMSTAT.DTR.ToString() });
                string comnoise = Properties.Settings.Default.COM_Noise;
                if (Enum.IsDefined(typeof(COMSTAT),comnoise))
                    cbb_COM_Noise.SelectedItem = comnoise;
                else
                    cbb_COM_Noise.SelectedItem = COMSTAT.RTS.ToString();

                cbb_COM_DUT.Items.AddRange(new string[] { COMSTAT.RTS.ToString(), COMSTAT.DTR.ToString() });
                string comdut = Properties.Settings.Default.COM_DUT;
                if (Enum.IsDefined(typeof(COMSTAT), comdut))
                    cbb_COM_DUT.SelectedItem = comdut;
                else
                    cbb_COM_DUT.SelectedItem = COMSTAT.DTR;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error while getting COM ports");
            }

            // setting up noise calibration
            if (Properties.Settings.Default.Noise_File_Activate)
            {
                rb_Noise_Cal_File.Checked = true;
                rb_Noise_Cal_Man.Checked = false;
            }
            else
            {
                rb_Noise_Cal_File.Checked = false;
                rb_Noise_Cal_Man.Checked = true;
            }
            InvalidateCalibration = false;

            // setup FFT filter combo box
            cbb_FFT_Filter.Items.Add(FFTALGORITHM.NONE);
            cbb_FFT_Filter.Items.Add(FFTALGORITHM.LOMONT);
            cbb_FFT_Filter.Items.Add(FFTALGORITHM.FFTW);
            try
            {
                if (Properties.Settings.Default.FFT_Filter)
                    cbb_FFT_Filter.SelectedItem = Properties.Settings.Default.FFT_Algorithm;
                else
                    cbb_FFT_Filter.SelectedItem = FFTALGORITHM.NONE;
            }
            catch
            {
                cbb_FFT_Filter.SelectedItem = FFTALGORITHM.NONE;
            }
            // setup tone output combo box
            cbb_Tone_Output.Items.Add(TONEOUTPUT.NONE);
            cbb_Tone_Output.Items.Add(TONEOUTPUT.NF);
            cbb_Tone_Output.Items.Add(TONEOUTPUT.GAIN);
            try
            {
                cbb_Tone_Output.SelectedItem = Properties.Settings.Default.Tone_Output;
            }
            catch
            {
                cbb_Tone_Output.SelectedItem = TONEOUTPUT.NONE;
            }
        }

        private void SettingsDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void btn_Noise_File_Click(object sender, EventArgs e)
        {
            OpenFileDialog Dlg = new OpenFileDialog();
            Dlg.CheckFileExists = true;
            Dlg.CheckPathExists = true;
            Dlg.Multiselect = false;
            Dlg.Filter = "Noise Calibration Files|*.cal";
            Dlg.DefaultExt = ".cal";
            if (Dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_Noise_FileName.Text = Dlg.FileName;
                InvalidateCalibration = true;
            }
        }

        private void cb_RTL_AGC_Auto_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_RTL_AGC_Auto.Checked)
            {
                cbb_RTL_TunerGain.Enabled = false;
            }
            else
            {
                cbb_RTL_TunerGain.Enabled = true;
            }
            InvalidateCalibration = true;
        }

        private void cbb_RTL_TunerGain_SelectedIndexChanged(object sender, EventArgs e)
        {
            int TunerGain = 0;
            if (cbb_RTL_TunerGain.SelectedItem != null)
            {
                // set tuner gain as selected in combo box
                TunerGain = (int)(((double)cbb_RTL_TunerGain.SelectedItem) * 10.0);
            }
            else
            {
                // use maximum gain from tuner properties
                if (cbb_Device.SelectedItem != null)
                {
                    TunerGain = ((RTLDevice)cbb_Device.SelectedItem).TunerGains[((RTLDevice)cbb_Device.SelectedItem).TunerGains.Count - 1];
                }
            }
            Properties.Settings.Default.RTL_TunerGain = TunerGain;
            InvalidateCalibration = true;
        }

        private void rb_Noise_Cal_Man_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Noise_Cal_Man.Checked)
            {
                Properties.Settings.Default.Noise_File_Activate = false;
                Properties.Settings.Default.Noise_FileName = "";
                btn_Noise_File.Enabled = false;
                tb_Noise_FileName.Enabled = false;
            }
            InvalidateCalibration = true;
        }

        private void rb_Noise_Cal_File_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Noise_Cal_File.Checked)
            {
                Properties.Settings.Default.Noise_File_Activate = true;
                btn_Noise_File.Enabled = true;
                tb_Noise_FileName.Enabled = true;
            }
            InvalidateCalibration = true;
        }

        private void cbb_Device_SelectedIndexChanged(object sender, EventArgs e)
        {
            // set the tuner type and gains according to the selceted tuner
            RTLDevice item = (RTLDevice)cbb_Device.SelectedItem;
            if (item == null)
                return;
            // save index of current selected device
            Properties.Settings.Default.RTL_DeviceIndex = item.Index;
            // set tuner type
            lbl_TunerType.Text = item.TunerType.ToString();
            // set up gain control
            try
            {
                // poulate combo box with gains
                cbb_RTL_TunerGain.Items.Clear();
                bool _gainfound = false;
                for (int i = 0; i < item.TunerGains.Count; i++)
                {
                    // add gain
                    cbb_RTL_TunerGain.Items.Add(((double)item.TunerGains[i]) / 10.0);
                    // check if gain is selected tuner gain
                    if (item.TunerGains[i] == Properties.Settings.Default.RTL_TunerGain)
                    {
                        cbb_RTL_TunerGain.SelectedIndex = cbb_RTL_TunerGain.Items.Count - 1;
                        _gainfound = true;
                    }
                }
                // set tuner gain to maximum if old setting is not found
                if (!_gainfound)
                {
                    cbb_RTL_TunerGain.SelectedIndex = cbb_RTL_TunerGain.Items.Count - 1;
                    Properties.Settings.Default.RTL_TunerGain = (int)((double)cbb_RTL_TunerGain.SelectedItem * 10.0);
                }
                cb_RTL_AGC_Auto_CheckedChanged(this, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error while getting tuner gains");
            }
            // set frequency bounds
            switch (item.TunerType)
            {
                case RtlSdrTunerType.E4000:
                    // set bounds
                    Properties.Settings.Default.RTL_Frequency_Min = 52;
                    Properties.Settings.Default.RTL_Frequency_Max = 2200;
                    // adjust current frequencies to new bounds
                    Properties.Settings.Default.RTL_Frequency = Math.Max(Properties.Settings.Default.RTL_Frequency, Properties.Settings.Default.RTL_Frequency_Min);
                    Properties.Settings.Default.RTL_Frequency = Math.Min(Properties.Settings.Default.RTL_Frequency, Properties.Settings.Default.RTL_Frequency_Max);
                    Properties.Settings.Default.RTL_Frequency_Start = Math.Max(Properties.Settings.Default.RTL_Frequency_Start, Properties.Settings.Default.RTL_Frequency_Min);
                    Properties.Settings.Default.RTL_Frequency_Start = Math.Min(Properties.Settings.Default.RTL_Frequency_Start, Properties.Settings.Default.RTL_Frequency_Max);
                    Properties.Settings.Default.RTL_Frequency_Stop = Math.Max(Properties.Settings.Default.RTL_Frequency_Stop, Properties.Settings.Default.RTL_Frequency_Min);
                    Properties.Settings.Default.RTL_Frequency_Stop = Math.Min(Properties.Settings.Default.RTL_Frequency_Stop, Properties.Settings.Default.RTL_Frequency_Max);
                    break;
                case RtlSdrTunerType.R820T:
                    Properties.Settings.Default.RTL_Frequency_Min = 24;
                    Properties.Settings.Default.RTL_Frequency_Max = 1766;
                    Properties.Settings.Default.RTL_Frequency = Math.Max(Properties.Settings.Default.RTL_Frequency, Properties.Settings.Default.RTL_Frequency_Min);
                    Properties.Settings.Default.RTL_Frequency = Math.Min(Properties.Settings.Default.RTL_Frequency, Properties.Settings.Default.RTL_Frequency_Max);
                    Properties.Settings.Default.RTL_Frequency_Start = Math.Max(Properties.Settings.Default.RTL_Frequency_Start, Properties.Settings.Default.RTL_Frequency_Min);
                    Properties.Settings.Default.RTL_Frequency_Start = Math.Min(Properties.Settings.Default.RTL_Frequency_Start, Properties.Settings.Default.RTL_Frequency_Max);
                    Properties.Settings.Default.RTL_Frequency_Stop = Math.Max(Properties.Settings.Default.RTL_Frequency_Stop, Properties.Settings.Default.RTL_Frequency_Min);
                    Properties.Settings.Default.RTL_Frequency_Stop = Math.Min(Properties.Settings.Default.RTL_Frequency_Stop, Properties.Settings.Default.RTL_Frequency_Max);
                    break;
                default:
                    Properties.Settings.Default.RTL_Frequency_Min = 0;
                    Properties.Settings.Default.RTL_Frequency_Max = 99999.999m;
                    Properties.Settings.Default.RTL_Frequency = Math.Max(Properties.Settings.Default.RTL_Frequency, Properties.Settings.Default.RTL_Frequency_Min);
                    Properties.Settings.Default.RTL_Frequency = Math.Min(Properties.Settings.Default.RTL_Frequency, Properties.Settings.Default.RTL_Frequency_Max);
                    Properties.Settings.Default.RTL_Frequency_Start = Math.Max(Properties.Settings.Default.RTL_Frequency_Start, Properties.Settings.Default.RTL_Frequency_Min);
                    Properties.Settings.Default.RTL_Frequency_Start = Math.Min(Properties.Settings.Default.RTL_Frequency_Start, Properties.Settings.Default.RTL_Frequency_Max);
                    Properties.Settings.Default.RTL_Frequency_Stop = Math.Max(Properties.Settings.Default.RTL_Frequency_Stop, Properties.Settings.Default.RTL_Frequency_Min);
                    Properties.Settings.Default.RTL_Frequency_Stop = Math.Min(Properties.Settings.Default.RTL_Frequency_Stop, Properties.Settings.Default.RTL_Frequency_Max);
                    break;
            }
            // invalidate calibrations
            InvalidateCalibration = true;
        }

        private void cb_RTL_AGC_CheckedChanged(object sender, EventArgs e)
        {
            InvalidateCalibration = true;
        }

        private void cbb_COM_Noise_SelectedIndexChanged(object sender, EventArgs e)
        {
            string stat = cbb_COM_Noise.SelectedItem.ToString();
            Properties.Settings.Default.COM_Noise = stat;
            if ((COMSTAT)Enum.Parse(typeof(COMSTAT), stat) == COMSTAT.RTS)
            {
                // change COM DUT selection if necessary
                cbb_COM_DUT.SelectedItem = COMSTAT.DTR.ToString();
            }
            else if ((COMSTAT)Enum.Parse(typeof(COMSTAT), stat) == COMSTAT.DTR)
            {
                // change COM DUT selection if necessary
                cbb_COM_DUT.SelectedItem = COMSTAT.RTS.ToString();
            }

        }

        private void cbb_COM_DUT_SelectedIndexChanged(object sender, EventArgs e)
        {
            string stat = cbb_COM_DUT.SelectedItem.ToString();
            Properties.Settings.Default.COM_DUT = stat;
            if ((COMSTAT)Enum.Parse(typeof(COMSTAT), stat) == COMSTAT.RTS)
            {
                // change COM Noise selection if necessary
                cbb_COM_Noise.SelectedItem = COMSTAT.DTR.ToString();
            }
            else if ((COMSTAT)Enum.Parse(typeof(COMSTAT), stat) == COMSTAT.DTR)
            {
                // change COM Noise selection if necessary
                cbb_COM_Noise.SelectedItem = COMSTAT.RTS.ToString();
            }
        }

        private void cbb_FFT_Filter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbb_FFT_Filter.SelectedItem != null)
            {
                Properties.Settings.Default.FFT_Algorithm = (FFTALGORITHM)cbb_FFT_Filter.SelectedItem;
                if (Properties.Settings.Default.FFT_Algorithm == FFTALGORITHM.NONE)
                    Properties.Settings.Default.FFT_Filter = false;
                else
                    Properties.Settings.Default.FFT_Filter = true;
            }
        }

        private void cbb_Tone_Output_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbb_Tone_Output.SelectedItem != null)
            {
                Properties.Settings.Default.Tone_Output = (TONEOUTPUT)cbb_Tone_Output.SelectedItem;
            }
        }

    }

}
