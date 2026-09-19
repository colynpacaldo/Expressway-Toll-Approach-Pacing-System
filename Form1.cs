using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FuzzyLogic
{
    public partial class Form1 : Form
    {
        private PictureBox graphBox;
        private double currentCrispSpeed = 0;

        private Label lblDistanceTitle, lblTrafficTitle, lblResult;
        private TrackBar tbDistance, tbTraffic;
        private Label lblDistanceValue, lblTrafficValue;
        private Button btnCalculate;
        private RichTextBox rtbLog;

        public Form1()
        {
            graphBox = new PictureBox();
            // Moved further right (X: 550) and made slightly taller
            graphBox.Size = new Size(400, 220);
            graphBox.Location = new Point(550, 40);
            graphBox.BackColor = Color.White;
            graphBox.BorderStyle = BorderStyle.FixedSingle;
            graphBox.Paint += DrawFuzzyGraph;
            this.Controls.Add(graphBox);

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Widened the form from 650 to 1000
            this.Text = "CCLEX Toll Approach Pacing System - Fuzzy Logic Controller";
            this.Size = new Size(1000, 750);
            this.Font = new Font("Segoe UI", 10);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            // Distance Input (Left Column)
            lblDistanceTitle = new Label { Text = "Distance to Toll Plaza (0 - 1000m):", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            tbDistance = new TrackBar { Minimum = 0, Maximum = 1000, Value = 350, Location = new Point(20, 50), Width = 450, TickFrequency = 50, BackColor = Color.WhiteSmoke };
            lblDistanceValue = new Label { Text = "350 m", Location = new Point(480, 50), AutoSize = true, Font = new Font("Segoe UI", 11) };
            tbDistance.Scroll += (s, e) => { lblDistanceValue.Text = tbDistance.Value + " m"; CalculateFuzzyLogic(); };

            // Traffic Input (Left Column)
            lblTrafficTitle = new Label { Text = "Traffic Density Ahead (0 - 100 vpm):", Location = new Point(20, 110), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            tbTraffic = new TrackBar { Minimum = 0, Maximum = 100, Value = 75, Location = new Point(20, 140), Width = 450, TickFrequency = 10, BackColor = Color.WhiteSmoke };
            lblTrafficValue = new Label { Text = "75 vpm", Location = new Point(480, 140), AutoSize = true, Font = new Font("Segoe UI", 11) };
            tbTraffic.Scroll += (s, e) => { lblTrafficValue.Text = tbTraffic.Value + " vpm"; CalculateFuzzyLogic(); };

            // Calculate Button (Sized to fit left column)
            btnCalculate = new Button { Text = "Run Mamdani Inference System", Location = new Point(20, 210), Width = 510, Height = 40, BackColor = Color.LightSteelBlue, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnCalculate.Click += (s, e) => CalculateFuzzyLogic();

            // Defuzzified Output Label
            lblResult = new Label { Text = "Suggested Speed: -- km/h", Location = new Point(20, 270), AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.DarkGreen };

            // Diagnostics Log (Expanded to span the entire bottom width)
            Label lblLogTitle = new Label { Text = "System Execution Trace:", Location = new Point(20, 320), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            rtbLog = new RichTextBox { Location = new Point(20, 345), Width = 930, Height = 340, ReadOnly = true, Font = new Font("Consolas", 9), BackColor = Color.White };

            // Add Controls
            this.Controls.Add(lblDistanceTitle);
            this.Controls.Add(tbDistance);
            this.Controls.Add(lblDistanceValue);
            this.Controls.Add(lblTrafficTitle);
            this.Controls.Add(tbTraffic);
            this.Controls.Add(lblTrafficValue);
            this.Controls.Add(btnCalculate);
            this.Controls.Add(lblResult);
            this.Controls.Add(lblLogTitle);
            this.Controls.Add(rtbLog);

            // Run initial calculation for default values
            CalculateFuzzyLogic();
        }

        // Universal function for Trapezoidal and Triangular membership functions
        // Triangular is achieved when parameter b == c
        private double Trap(double x, double a, double b, double c, double d)
        {
            if (x < a || x > d) return 0.0;
            if (x >= b && x <= c) return 1.0;

            double leftSlope = (b > a) ? (x - a) / (b - a) : 1.0;
            double rightSlope = (d > c) ? (d - x) / (d - c) : 1.0;

            if (x < b) return leftSlope;
            if (x > c) return rightSlope;
            return 0.0;
        }

        private void CalculateFuzzyLogic()
        {
            double dist = tbDistance.Value;
            double traf = tbTraffic.Value;

            StringBuilder log = new StringBuilder();

            // ==========================================
            // STEP A: FUZZIFICATION
            // ==========================================
            // Distance parameters
            double dNear = Trap(dist, 0, 0, 200, 500);
            double dMed = Trap(dist, 200, 500, 500, 800);
            double dFar = Trap(dist, 500, 800, 1000, 1000);

            // Traffic parameters
            double tLight = Trap(traf, 0, 0, 30, 60);
            double tMod = Trap(traf, 30, 50, 50, 80);
            double tHeavy = Trap(traf, 50, 80, 100, 100);

            log.AppendLine($"=== Step A: Fuzzification ===");
            log.AppendLine($"Distance ({dist}m) -> Near: {dNear:F2} | Medium: {dMed:F2} | Far: {dFar:F2}");
            log.AppendLine($"Traffic  ({traf}vpm) -> Light: {tLight:F2} | Moderate: {tMod:F2} | Heavy: {tHeavy:F2}\n");

            // ==========================================
            // STEP B: RULE IMPLICATION (MIN Operator)
            // ==========================================
            double r1 = Math.Min(dNear, tLight);  // Output: Slow
            double r2 = Math.Min(dNear, tMod);    // Output: Slow
            double r3 = Math.Min(dNear, tHeavy);  // Output: Slow

            double r4 = Math.Min(dMed, tLight);   // Output: Coast
            double r5 = Math.Min(dMed, tMod);     // Output: Coast
            double r6 = Math.Min(dMed, tHeavy);   // Output: Slow

            double r7 = Math.Min(dFar, tLight);   // Output: Cruise
            double r8 = Math.Min(dFar, tMod);     // Output: Coast
            double r9 = Math.Min(dFar, tHeavy);   // Output: Slow

            log.AppendLine($"=== Step B: Rule Implication (Active Rules) ===");
            if (r1 > 0) log.AppendLine($"R1 (Near AND Light)      -> Slow   = MIN({dNear:F2}, {tLight:F2}) = {r1:F2}");
            if (r2 > 0) log.AppendLine($"R2 (Near AND Moderate)   -> Slow   = MIN({dNear:F2}, {tMod:F2}) = {r2:F2}");
            if (r3 > 0) log.AppendLine($"R3 (Near AND Heavy)      -> Slow   = MIN({dNear:F2}, {tHeavy:F2}) = {r3:F2}");
            if (r4 > 0) log.AppendLine($"R4 (Medium AND Light)    -> Coast  = MIN({dMed:F2}, {tLight:F2}) = {r4:F2}");
            if (r5 > 0) log.AppendLine($"R5 (Medium AND Moderate) -> Coast  = MIN({dMed:F2}, {tMod:F2}) = {r5:F2}");
            if (r6 > 0) log.AppendLine($"R6 (Medium AND Heavy)    -> Slow   = MIN({dMed:F2}, {tHeavy:F2}) = {r6:F2}");
            if (r7 > 0) log.AppendLine($"R7 (Far AND Light)       -> Cruise = MIN({dFar:F2}, {tLight:F2}) = {r7:F2}");
            if (r8 > 0) log.AppendLine($"R8 (Far AND Moderate)    -> Coast  = MIN({dFar:F2}, {tMod:F2}) = {r8:F2}");
            if (r9 > 0) log.AppendLine($"R9 (Far AND Heavy)       -> Slow   = MIN({dFar:F2}, {tHeavy:F2}) = {r9:F2}");

            // ==========================================
            // STEP C: AGGREGATION (MAX Operator)
            // ==========================================
            double outSlow = Math.Max(r1, Math.Max(r2, Math.Max(r3, Math.Max(r6, r9))));
            double outCoast = Math.Max(r4, Math.Max(r5, r8));
            double outCruise = r7;

            log.AppendLine($"\n=== Step C: Aggregation (MAX) ===");
            log.AppendLine($"Aggregated Slow clipping height   : {outSlow:F2}");
            log.AppendLine($"Aggregated Coast clipping height  : {outCoast:F2}");
            log.AppendLine($"Aggregated Cruise clipping height : {outCruise:F2}\n");

            // ==========================================
            // STEP D: DEFUZZIFICATION (Centroid Method)
            // ==========================================
            double numerator = 0;
            double denominator = 0;

            // Integrate over the output range of 0 to 80 km/h with a 1 km/h step
            for (double v = 0; v <= 80; v += 1.0)
            {
                // Recreate output membership shapes
                double mSlow = Trap(v, 0, 0, 0, 40);
                double mCoast = Trap(v, 20, 40, 40, 60);
                double mCruise = Trap(v, 40, 80, 80, 80);

                // Clip output shapes based on aggregation heights
                double cSlow = Math.Min(mSlow, outSlow);
                double cCoast = Math.Min(mCoast, outCoast);
                double cCruise = Math.Min(mCruise, outCruise);

                // Merge into final complex polygon
                double cTotal = Math.Max(cSlow, Math.Max(cCoast, cCruise));

                numerator += v * cTotal;
                denominator += cTotal;
            }

            double finalSpeed = (denominator == 0) ? 0 : (numerator / denominator);

            log.AppendLine($"=== Step D: Defuzzification (Centroid) ===");
            log.AppendLine($"Numerator (Sum of x * y)   : {numerator:F2}");
            log.AppendLine($"Denominator (Area)         : {denominator:F2}");
            log.AppendLine($"Center of Gravity (Speed)  : {finalSpeed:F2} km/h");

            // Update UI
            lblResult.Text = $"Suggested Speed: {finalSpeed:F1} km/h";
            rtbLog.Text = log.ToString();

            // Scroll to top of log for easy reading
            rtbLog.SelectionStart = 0;
            rtbLog.ScrollToCaret();

            currentCrispSpeed = finalSpeed;
            graphBox.Invalidate();
        }

        private void DrawFuzzyGraph(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int w = graphBox.Width;
            int h = graphBox.Height;

            // Draw the bottom X-Axis line
            g.DrawLine(Pens.Gray, 0, h - 20, w, h - 20);

            // Helper function to translate Speed (0-100) and Membership (0.0-1.0) into pixel coordinates
            PointF GetPoint(double speed, double truth)
            {
                float px = (float)(speed / 100.0 * w);
                float py = (float)(h - 20 - (truth * (h - 40)));
                return new PointF(px, py);
            }

            // 1. Draw "Slow" Membership Function (Blue)
            PointF[] slowPts = { GetPoint(0, 1), GetPoint(20, 1), GetPoint(40, 0) };
            g.DrawLines(Pens.Blue, slowPts);
            g.DrawString("Slow", this.Font, Brushes.Blue, GetPoint(5, 1.05));

            // 2. Draw "Medium" Membership Function (Green)
            PointF[] medPts = { GetPoint(20, 0), GetPoint(50, 1), GetPoint(80, 0) };
            g.DrawLines(Pens.Green, medPts);
            g.DrawString("Medium", this.Font, Brushes.Green, GetPoint(40, 1.05));

            // 3. Draw "Fast" Membership Function (Orange)
            PointF[] fastPts = { GetPoint(60, 0), GetPoint(80, 1), GetPoint(100, 1) };
            g.DrawLines(Pens.Orange, fastPts);
            g.DrawString("Fast", this.Font, Brushes.Orange, GetPoint(80, 1.05));

            // 4. Draw the Crisp Output (Defuzzified Speed) as a Red Line
            if (currentCrispSpeed > 0)
            {
                float lineX = (float)(currentCrispSpeed / 100.0 * w);
                using (Pen redPen = new Pen(Color.Red, 2))
                {
                    g.DrawLine(redPen, lineX, 0, lineX, h - 20);
                }

                // Add a floating text label for the exact speed next to the red line
                g.DrawString($"Final: {currentCrispSpeed:F1} km/h",
                             new Font(this.Font, FontStyle.Bold),
                             Brushes.Red, lineX + 5, 10);
            }
        }
    }
}