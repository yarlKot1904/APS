namespace APS.Simulators
{
    public class SimulationResultsForm : Form
    {
        private Label finalNLabel;
        private Label finalPLabel;

        public SimulationResultsForm(Simulation simulation)
        {
            this.Text = "Simulation Results";
            this.Size = new Size(1200, 600);

            DataGridView sourcesGrid = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(1140, 250),
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            sourcesGrid.Columns.Add("Source", "Источник");
            sourcesGrid.Columns.Add("Count", "Количество заявок");
            sourcesGrid.Columns.Add("RejectionProbability", "Вероятность отказа");
            sourcesGrid.Columns.Add("AvgWaitTime", "Среднее время ожидания");
            sourcesGrid.Columns.Add("AvgServiceTime", "Среднее время обслуживания");
            sourcesGrid.Columns.Add("AvgBufferTime", "Среднее время в буфере");
            sourcesGrid.Columns.Add("AvgSystemTime", "Среднее время в СМО");
            sourcesGrid.Columns.Add("ServiceVariance", "Дисперсия обслуживания");
            sourcesGrid.Columns.Add("BufferVariance", "Дисперсия времени в буфере");
            sourcesGrid.Columns.Add("DeviceUtilization", "Использование приборов");
            sourcesGrid.Columns.Add("UsedBuffers", "Использование буферов");

            int numSources = simulation.RefusalCounts.Length;
            for (int i = 0; i < numSources; i++)
            {
                double avgServiceTime = simulation.ServiceTimes[i].Count > 0 ? simulation.ServiceTimes[i].Average() : 0;
                double avgWaitTime = simulation.TotalWaitTimes[i] / simulation.AllRequests.Count;
                double avgBufferTime = simulation.BufferWaitTimes[i].Count > 0 ? simulation.BufferWaitTimes[i].Average() : 0;
                double avgSystemTime = avgWaitTime + avgServiceTime;
                double serviceVariance = simulation.ServiceTimes[i].Count > 0 ? simulation.ServiceTimes[i].Select(x => Math.Pow(x - avgServiceTime, 2)).Average() : 0;
                double bufferVariance = simulation.BufferWaitTimes[i].Count > 0 ? simulation.BufferWaitTimes[i].Select(x => Math.Pow(x - avgBufferTime, 2)).Average() : 0;
                double deviceUtilization = simulation.DeviceUsageTimesBySources[i] / (simulation.CurrentTime);
                string usedBuffers = simulation.BufferWaitTimes[i].Count > 0 ? "Да" : "Нет";

                sourcesGrid.Rows.Add(
                    $"И{i + 1}",
                    simulation.MaxRequests,
                    $"{(double)simulation.RefusalCounts[i] / simulation.TotalRequestsTimes[i]:F4}",
                    $"{avgWaitTime:F4}",
                    $"{avgServiceTime:F4}",
                    $"{avgBufferTime:F4}",
                    $"{avgSystemTime:F4}",
                    $"{serviceVariance:F4}",
                    $"{bufferVariance:F4}",
                    $"{deviceUtilization:F4}",
                    usedBuffers
                );
            }

            DataGridView devicesGrid = new DataGridView
            {
                Location = new Point(20, 300),
                Size = new Size(1140, 200),
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            devicesGrid.Columns.Add("Device", "Прибор");
            devicesGrid.Columns.Add("Utilization", "Коэффициент использования");

            for (int i = 0; i < simulation.Devices.Count; i++)
            {
                double utilization = simulation.DeviceUsageTimes[i] / (simulation.CurrentTime * 1.0);
                devicesGrid.Rows.Add(
                    $"П{i + 1}",
                    $"{utilization:F4}"
                );
            }

            Controls.Add(sourcesGrid);
            Controls.Add(devicesGrid);

            finalNLabel = new Label
            {
                Text = $"Final N: {simulation.FinalSampleSize}",
                Location = new Point(20, 520),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            this.Controls.Add(finalNLabel);

            finalPLabel = new Label
            {
                Text = $"Estimated p(A): {simulation.FinalRefusalProbability:F4}",
                Location = new Point(20, 560),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            this.Controls.Add(finalPLabel);
        }
    }



}
