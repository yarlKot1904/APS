using APS.Simulators;
using APS.Helper;

namespace APS
{
    public class MainForm : Form
    {
        private Button startButton;
        private Button stepButton;
        private Button autoButton;
        private VisualSimulator visualSimulator;
        private Simulation simulation;
        private bool autoMode;
        private SimulationConfig config;
        private Button determineNButton;

        private ListBox iterationsListBox;


        public MainForm()
        {
            this.Text = "Simulation Model";
            this.Size = new Size(1200, 800);

            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs/Config.json");
            config = ConfigLoader.LoadConfig(configFilePath);

            visualSimulator = new VisualSimulator(this, config.NumberOfSources, config.NumberOfBuffers, config.NumberOfDevices);

            startButton = new Button
            {
                Text = "Начать симуляцию",
                Location = new Point(920, 20),
                Size = new Size(200, 40)
            };
            startButton.Click += StartButton_Click;
            this.Controls.Add(startButton);

            stepButton = new Button
            {
                Text = "Следующий шаг",
                Location = new Point(920, 80),
                Size = new Size(200, 40),
                Enabled = false
            };
            stepButton.Click += StepButton_Click;
            this.Controls.Add(stepButton);

            autoButton = new Button
            {
                Text = "Автоматический режим",
                Location = new Point(920, 140),
                Size = new Size(200, 40),
                Enabled = false
            };
            autoButton.Click += AutoButton_Click;
            this.Controls.Add(autoButton);

            determineNButton = new Button
            {
                Text = "Определить N",
                Location = new System.Drawing.Point(920, 200),
                Size = new System.Drawing.Size(200, 40),
                Enabled = false
            };
            determineNButton.Click += DetermineNButton_Click;
            this.Controls.Add(determineNButton);

            iterationsListBox = new ListBox
            {
                Location = new Point(920, 260),
                Size = new Size(200, 400)
            };
            this.Controls.Add(iterationsListBox);

        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            simulation = new Simulation(
                config.NumberOfSources,
                config.NumberOfBuffers,
                config.NumberOfDevices,
                config.MaxRequests,
                config.MeanServiceTime,
                config.MinInterArrivalTime,
                config.MaxInterArrivalTime,
                config.BufferCapacity,
                config.ConfidenceLevel,
                config.RelativePrecision,
                visualSimulator
            );

            stepButton.Enabled = true;
            autoButton.Enabled = true;
            determineNButton.Enabled = true;
            startButton.Enabled = false;
        }

        private async void DetermineNButton_Click(object sender, EventArgs e)
        {
            if (simulation != null)
            {
                determineNButton.Enabled = false;
                stepButton.Enabled = false;
                autoButton.Enabled = false;
                startButton.Enabled = false;

                simulation.OnIterationCompleted += (iteration, p, N) =>
                {
                    
                    this.Invoke((MethodInvoker)delegate
                    {
                        int result = N;
                        iterationsListBox.Items.Add($"Итерация {iteration}: p(A) = {p:F4}, N = {result}");
                    });
                };


                await Task.Run(() => simulation.DetermineOptimalSampleSize());

                simulation.OnIterationCompleted -= (iteration, p, n) => { };

                determineNButton.Enabled = true;
                stepButton.Enabled = true;
                autoButton.Enabled = true;
                startButton.Enabled = false; 

                simulation.ShowResults();
            }
        }





        private void StepButton_Click(object sender, EventArgs e)
        {
            if (simulation != null)
            {
                simulation.Step();

                if (simulation.AllRequests.Count >= simulation.MaxRequests &&
                    !simulation.Devices.Exists(d => d.IsBusy) &&
                    simulation.Buffers.All(b => b.Count == 0))
                {
                    stepButton.Enabled = false;
                    autoButton.Enabled = false;
                    simulation.ShowResults();
                }
            }
        }

        private void AutoButton_Click(object sender, EventArgs e)
        {
            if (simulation != null)
            {
                autoMode = true;
                simulation.AutoMode = autoMode;
                simulation.RunAuto();
                stepButton.Enabled = false;
                autoButton.Enabled = false;
            }
        }
    }
}
