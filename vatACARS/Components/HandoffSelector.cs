﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using vatACARS.Util;
using System.Windows.Forms;
using vatsys;
using static vatACARS.Helpers.Tranceiver;
using System.Drawing;
using System.Linq;
namespace vatACARS.Components
{
    public partial class HandoffSelector : BaseForm
    {
        private Station selectedStation;
        private static Logger logger = new Logger("HandoffSelector");
        private static Label SelectedDataAuthority;
        private static Label SelectedSector;
        private static HttpClient client = new HttpClient();
        private static StationInformation[] OnlineStations;
        private Dictionary<string, Sector[]> StationSectors = new Dictionary<string, Sector[]>();

        public HandoffSelector()
        {
            InitializeComponent();
            selectedStation = DispatchWindow.SelectedStation;
            OnlineStations = VatACARSInterface.stationsOnline;

            FetchOnlineATSUs();

            StyleComponent();
        }

        private void clearDataAuthorities()
        {
            tbl_dataAuthorities.Controls.Clear();
            Label noneButton = new Label();
            noneButton.Text = "(NONE)";
            noneButton.Size = new Size(104, 30);
            noneButton.Font = MMI.eurofont_winsml;
            noneButton.TextAlign = ContentAlignment.MiddleCenter;
            noneButton.ForeColor = Colours.GetColour(Colours.Identities.CPDLCMessageBackground);
            noneButton.BackColor = Colours.GetColour(Colours.Identities.CPDLCUplink);
            noneButton.Margin = new Padding(3); // A bit of spacing
            noneButton.Parent = tbl_dataAuthorities;

            noneButton.MouseEnter += (sender, e) => noneButton.BackColor = Colours.GetColour(SelectedDataAuthority == noneButton ? Colours.Identities.CPDLCUplink : Colours.Identities.CPDLCDownlink);
            noneButton.MouseLeave += (sender, e) => noneButton.BackColor = Colours.GetColour(SelectedDataAuthority == noneButton ? Colours.Identities.CPDLCDownlink : Colours.Identities.CPDLCUplink);

            noneButton.MouseUp += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (SelectedDataAuthority == noneButton) return;
                    if (SelectedDataAuthority != null) SelectedDataAuthority.BackColor = Colours.GetColour(Colours.Identities.CPDLCUplink);
                    SelectedDataAuthority = noneButton;
                    noneButton.BackColor = Colours.GetColour(Colours.Identities.CPDLCUplink);

                    clearSectors(true);
                    // Add vatsys network stations
                    List<SectorsVolumes.Sector> sectors = (from s in SectorsVolumes.Sectors
                                                       where s.HandoffEligible
                                                       orderby Network.GetOnlineATCs.Any((NetworkATC a) => a.Callsign == s.Callsign) descending,
                                                       s.Name
                                                       select s).ToList();
                    foreach (SectorsVolumes.Sector sector in sectors)
                    {
                        Label sectorBtn = new Label();
                        sectorBtn.Text = $"{sector.Name} {(sector.Frequency / 1000000.0).ToString("0.0##")}";
                        sectorBtn.Tag = sector;
                        sectorBtn.Size = new Size(130, 30);
                        sectorBtn.Font = MMI.eurofont_winsml;
                        sectorBtn.TextAlign = ContentAlignment.MiddleCenter;
                        sectorBtn.ForeColor = Colours.GetColour(Colours.Identities.CPDLCMessageBackground);
                        sectorBtn.BackColor = Colours.GetColour(Colours.Identities.CPDLCUplink);
                        sectorBtn.Margin = new Padding(3); // A bit of spacing
                        sectorBtn.Parent = tbl_sectors;

                        sectorBtn.MouseEnter += (sender2, e2) => sectorBtn.BackColor = Colours.GetColour(SelectedSector == sectorBtn ? Colours.Identities.CPDLCUplink : Colours.Identities.CPDLCDownlink);
                        sectorBtn.MouseLeave += (sender2, e2) => sectorBtn.BackColor = Colours.GetColour(SelectedSector == sectorBtn ? Colours.Identities.CPDLCDownlink : Colours.Identities.CPDLCUplink);

                        sectorBtn.MouseDown += (sender2, e2) =>
                        {
                            if (e2.Button == MouseButtons.Left)
                            {
                                if (SelectedSector == sectorBtn) return;
                                if (SelectedSector != null) SelectedSector.BackColor = Colours.GetColour(Colours.Identities.CPDLCUplink);
                                SelectedSector = sectorBtn;
                                sectorBtn.BackColor = Colours.GetColour(Colours.Identities.WindowButtonSelected);
                            }
                        };
                    }
                }
            };
        }

        private void clearSectors(bool addNoneButton = false)
        {
            tbl_sectors.Controls.Clear();

            if (addNoneButton)
            {
                Label noneButton = new Label();
                noneButton.Text = "(NONE)";
                noneButton.Size = new Size(130, 30);
                noneButton.Font = MMI.eurofont_winsml;
                noneButton.TextAlign = ContentAlignment.MiddleCenter;
                noneButton.ForeColor = Colours.GetColour(Colours.Identities.CPDLCMessageBackground);
                noneButton.BackColor = Colours.GetColour(Colours.Identities.CPDLCUplink);
                noneButton.Margin = new Padding(3); // A bit of spacing
                noneButton.Parent = tbl_sectors;

                noneButton.MouseEnter += (sender, e) => noneButton.BackColor = Colours.GetColour(SelectedSector == noneButton ? Colours.Identities.CPDLCUplink : Colours.Identities.CPDLCDownlink);
                noneButton.MouseLeave += (sender, e) => noneButton.BackColor = Colours.GetColour(SelectedSector == noneButton ? Colours.Identities.CPDLCDownlink : Colours.Identities.CPDLCUplink);

                noneButton.MouseUp += (sender, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if (SelectedSector == noneButton) return;
                        if (SelectedSector != null) SelectedSector.BackColor = Colours.GetColour(Colours.Identities.CPDLCUplink);
                        SelectedSector = noneButton;
                        noneButton.BackColor = Colours.GetColour(Colours.Identities.WindowButtonSelected);
                    }
                };
            }
        }

        // TODO: This should be done periodically and stored somewhere
        private void FetchOnlineATSUs()
        {
            clearDataAuthorities();
            clearSectors();

            foreach (StationInformation station in OnlineStations)
            {
                Label btn = new Label();
                btn.Text = station.Station_Code;
                btn.Tag = station;
                btn.Size = new Size(104, 30);
                btn.Font = MMI.eurofont_winsml;
                btn.TextAlign = ContentAlignment.MiddleCenter;
                btn.ForeColor = Colours.GetColour(Colours.Identities.CPDLCMessageBackground);
                btn.BackColor = Colours.GetColour(Colours.Identities.CPDLCUplink);
                btn.Margin = new Padding(3); // A bit of spacing
                btn.Parent = tbl_dataAuthorities;

                btn.MouseEnter += (sender, e) => btn.BackColor = Colours.GetColour(SelectedDataAuthority == btn ? Colours.Identities.CPDLCUplink : Colours.Identities.CPDLCDownlink);
                btn.MouseLeave += (sender, e) => btn.BackColor = Colours.GetColour(SelectedDataAuthority == btn ? Colours.Identities.CPDLCDownlink : Colours.Identities.CPDLCUplink);

                btn.MouseDown += (sender, e) =>
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            if (SelectedDataAuthority == btn) return;
                            if (SelectedDataAuthority != null) SelectedDataAuthority.BackColor = Colours.GetColour(Colours.Identities.CPDLCUplink);
                            SelectedDataAuthority = btn;
                            btn.BackColor = Colours.GetColour(Colours.Identities.WindowButtonSelected);

                            clearSectors();

                            if (StationSectors.ContainsKey(station.Station_Code))
                            {
                                foreach (Sector sector in StationSectors[station.Station_Code])
                                {
                                    if (sector.Frequency == "0") continue;
                                    Label sectorBtn = new Label();
                                    sectorBtn.Text = $"{sector.Name} {(long.Parse(sector.Frequency) / 1000000.0).ToString("0.0##")}";
                                    sectorBtn.Tag = sector;
                                    sectorBtn.Size = new Size(130, 30);
                                    sectorBtn.Font = MMI.eurofont_winsml;
                                    sectorBtn.TextAlign = ContentAlignment.MiddleCenter;
                                    sectorBtn.ForeColor = Colours.GetColour(Colours.Identities.CPDLCMessageBackground);
                                    sectorBtn.BackColor = Colours.GetColour(Colours.Identities.CPDLCUplink);
                                    sectorBtn.Margin = new Padding(3); // A bit of spacing
                                    sectorBtn.Parent = tbl_sectors;

                                    sectorBtn.MouseEnter += (sender2, e2) => sectorBtn.BackColor = Colours.GetColour(SelectedSector == sectorBtn ? Colours.Identities.CPDLCUplink : Colours.Identities.CPDLCDownlink);
                                    sectorBtn.MouseLeave += (sender2, e2) => sectorBtn.BackColor = Colours.GetColour(SelectedSector == sectorBtn ? Colours.Identities.CPDLCDownlink : Colours.Identities.CPDLCUplink);

                                    sectorBtn.MouseDown += (sender2, e2) =>
                                    {
                                        if (e2.Button == MouseButtons.Left)
                                        {
                                            if (SelectedSector == sectorBtn) return;
                                            if (SelectedSector != null) SelectedSector.BackColor = Colours.GetColour(Colours.Identities.CPDLCUplink);
                                            SelectedSector = sectorBtn;
                                            sectorBtn.BackColor = Colours.GetColour(Colours.Identities.WindowButtonSelected);
                                        }
                                    };
                                }
                            }
                        }
                    };

                StationSectors.Add(station.Station_Code, JsonConvert.DeserializeObject<Sector[]>(station.Sectors));
            }
        }

        private void StyleComponent()
        {
            Text = selectedStation.Callsign;
        }

        private void btn_logoff_Click(object sender, System.EventArgs e)
        {
            FormUrlEncodedContent req = HoppiesInterface.ConstructMessage(selectedStation.Callsign, "CPDLC", $"/data2/{SentMessages}//N/LOGOFF");
            _ = HoppiesInterface.SendMessage(req);
            selectedStation.removeStation();
            Close();
        }

        private void btn_handoff_Click(object sender, EventArgs e)
        {
            string dataAuthority = SelectedDataAuthority.Tag == null ? "END SERVICE" : $"NEXT DATA AUTHORITY {((StationInformation)SelectedDataAuthority.Tag).Station_Code}";
            string sector = SelectedSector.Tag == null ? "MONITOR UNICOM 122.8" : $"CONTACT @{((Sector)SelectedSector.Tag).Callsign}@ @{(long.Parse(((Sector)SelectedSector.Tag).Frequency) / 1000000.0).ToString("0.0##")}@";
            string encodedMessage = $"{dataAuthority}\n{sector}";
            FormUrlEncodedContent req = HoppiesInterface.ConstructMessage(selectedStation.Callsign, "CPDLC", $"/data2/{SentMessages}//WU/{encodedMessage}");
            _ = HoppiesInterface.SendMessage(req);

            addSentCPDLCMessage(new SentCPDLCMessage()
            {
                Station = selectedStation.Callsign,
                MessageId = SentMessages,
                ReplyMessageId = SentMessages
            });

            addCPDLCMessage(new CPDLCMessage()
            {
                State = 2,
                Station = selectedStation.Callsign,
                Content = encodedMessage.Replace("@", "").Replace("\n", ", "),
                TimeReceived = DateTime.Now,
                MessageId = SentMessages,
                ReplyMessageId = -1
            });
            Close();
        }
    }
}
