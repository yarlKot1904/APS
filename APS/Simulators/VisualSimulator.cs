using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace APS.Simulators
{
    public class VisualSimulator
    {
        private Form mainForm;
        private Panel sourcesPanel;
        private Panel buffersPanel;
        private Panel devicesPanel;
        private Panel refusePanel;

        private Dictionary<int, Label> sourceLabels;
        private Dictionary<int, Label> bufferLabels;
        private Dictionary<int, Label> deviceLabels;
        private Label refuseLabel;

        public VisualSimulator(Form form, int numSources, int numBuffers, int numDevices)
        {
            mainForm = form;

            sourcesPanel = new Panel { Location = new Point(20, 20), Size = new Size(200, 700), AutoScroll = true };
            buffersPanel = new Panel { Location = new Point(240, 20), Size = new Size(200, 700), AutoScroll = true };
            devicesPanel = new Panel { Location = new Point(460, 20), Size = new Size(200, 700), AutoScroll = true };
            refusePanel = new Panel { Location = new Point(680, 20), Size = new Size(200, 700), AutoScroll = true };

            mainForm.Controls.Add(sourcesPanel);
            mainForm.Controls.Add(buffersPanel);
            mainForm.Controls.Add(devicesPanel);
            mainForm.Controls.Add(refusePanel);

            sourceLabels = new Dictionary<int, Label>();
            bufferLabels = new Dictionary<int, Label>();
            deviceLabels = new Dictionary<int, Label>();

            for (int i = 0; i < numSources; i++)
            {
                Label lbl = new Label
                {
                    Text = $"Source {i + 1}: ",
                    Location = new Point(10, 20 + i * 30),
                    AutoSize = true
                };
                sourcesPanel.Controls.Add(lbl);
                sourceLabels.Add(i, lbl);
            }

            for (int i = 0; i < numBuffers; i++)
            {
                Label lbl = new Label
                {
                    Text = $"Buffer {i + 1}: ",
                    Location = new Point(10, 20 + i * 30),
                    AutoSize = true
                };
                buffersPanel.Controls.Add(lbl);
                bufferLabels.Add(i, lbl);
            }

            for (int i = 0; i < numDevices; i++)
            {
                Label lbl = new Label
                {
                    Text = $"Device {i + 1}: Free",
                    Location = new Point(10, 20 + i * 30),
                    AutoSize = true
                };
                devicesPanel.Controls.Add(lbl);
                deviceLabels.Add(i, lbl);
            }

            refuseLabel = new Label
            {
                Text = $"Refuse: ",
                Location = new Point(10, 20),
                AutoSize = true
            };
            refusePanel.Controls.Add(refuseLabel);
        }

        private void SafeInvoke(Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => action()));
            }
            else
            {
                action();
            }
        }


        public void ClearAll()
        {
            foreach (var lbl in sourceLabels.Values)
            {
                SafeInvoke(lbl, () => lbl.Text = lbl.Text.Split(':')[0] + ": ");
            }

            foreach (var lbl in bufferLabels.Values)
            {
                SafeInvoke(lbl, () => lbl.Text = lbl.Text.Split(':')[0] + ": ");
            }

            foreach (var lbl in deviceLabels.Values)
            {
                SafeInvoke(lbl, () => lbl.Text = lbl.Text.Split(':')[0] + ": Free");
            }

            SafeInvoke(refuseLabel, () => refuseLabel.Text = "Refuse: ");
            Console.WriteLine("Визуализация очищена.");
        }

        public void AddRequest(string requestId, int sourceIndex)
        {
            if (sourceLabels.ContainsKey(sourceIndex))
            {
                var lbl = sourceLabels[sourceIndex];
                SafeInvoke(lbl, () => lbl.Text = $"Source {sourceIndex + 1}: {requestId}");
                Console.WriteLine($"Заявка {requestId} добавлена к Source {sourceIndex + 1}");
            }
        }

        public void MoveRequestToDevice(string requestId, int deviceIndex)
        {
            if (deviceLabels.ContainsKey(deviceIndex))
            {
                var lbl = deviceLabels[deviceIndex];
                SafeInvoke(lbl, () => lbl.Text = $"Device {deviceIndex + 1}: {requestId}");
                Console.WriteLine($"Заявка {requestId} перемещена на Device {deviceIndex + 1}");
            }
        }

        public void MoveRequestToBuffer(string requestId, int bufferIndex)
        {
            if (bufferLabels.ContainsKey(bufferIndex))
            {
                var lbl = bufferLabels[bufferIndex];
                SafeInvoke(lbl, () => lbl.Text = $"Buffer {bufferIndex + 1}: {requestId}");
                Console.WriteLine($"Заявка {requestId} перемещена в Buffer {bufferIndex + 1}");
            }
        }

        public void MoveRequestToRefuse(string requestId)
        {
            SafeInvoke(refuseLabel, () => refuseLabel.Text = $"Refuse: {requestId}");
            Console.WriteLine($"Заявка {requestId} отказана");
        }

        public void ClearDevice(int deviceIndex)
        {
            if (deviceLabels.ContainsKey(deviceIndex))
            {
                var lbl = deviceLabels[deviceIndex];
                SafeInvoke(lbl, () => lbl.Text = $"Device {deviceIndex + 1}: Free");
                Console.WriteLine($"Device {deviceIndex + 1} освобожден");
            }
        }

        public void ClearBuffer(int bufferIndex)
        {
            if (bufferLabels.ContainsKey(bufferIndex))
            {
                var lbl = bufferLabels[bufferIndex];
                SafeInvoke(lbl, () => lbl.Text = $"Buffer {bufferIndex + 1}: ");
                Console.WriteLine($"Buffer {bufferIndex + 1} очищен");
            }
        }

        public void ClearSource(int sourceIndex)
        {
            if (sourceLabels.ContainsKey(sourceIndex))
            {
                var lbl = sourceLabels[sourceIndex];
                SafeInvoke(lbl, () => lbl.Text = $"Source {sourceIndex + 1}: ");
                Console.WriteLine($"Source {sourceIndex + 1} очищен");
            }
        }
    }
}
