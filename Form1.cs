using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FuzzyLogic
{
    public partial class Form1 : Form
    {
        private PictureBox pbDistance, pbTraffic, pbOutput;
        private double currentCrispSpeed = 0;

        private Label lblHeader, lblDistanceTitle, lblTrafficTitle, lblResult;
        private TrackBar tbDistance, tbTraffic;
        private Label lblDistanceValue, lblTrafficValue;
        private RichTextBox rtbLog;

        public Form1()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Taller form to accommodate 3 stacked graphs
            this.Text = "CCLEX Toll Approach Pacing System - Fuzzy Logic Controller";
            this.Size = new Size(1000, 850);
            this.Font = new Font("Segoe UI", 10);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            // Header Label (Replaced Button at top left)
            lblHeader = new Label { Text = "Run Mamdani Inference System", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.DarkSlateBlue };

            // Distance Input
            lblDistanceTitle = new Label { Text = "Distance to Toll Plaza (0 - 1000m):", Location = new Point(20, 70), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            tbDistance = new TrackBar { Minimum = 0, Maximum = 1000, Value = 350, Location = new Point(20, 100), Width = 450, TickFrequency = 50, BackColor = Color.WhiteSmoke };
            lblDistanceValue = new Label { Text = "350 m", Location = new Point(480, 100), AutoSize = true, Font = new Font("Segoe UI", 11) };
            tbDistance.Scroll += (s, e) => { lblDistanceValue.Text = tbDistance.Value + " m"; CalculateFuzzyLogic(); };

            // Traffic Input
            lblTrafficTitle = new Label { Text = "Traffic Density Ahead (0 - 100 vpm):", Location = new Point(20, 160), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            tbTraffic = new TrackBar { Minimum = 0, Maximum = 100, Value = 75, Location = new Point(20, 190), Width = 450, TickFrequency = 10, BackColor = Color.WhiteSmoke };
            lblTrafficValue = new Label { Text = "75 vpm", Location = new Point(480, 190), AutoSize = true, Font = new Font("Segoe UI", 11) };
            tbTraffic.Scroll += (s, e) => { lblTrafficValue.Text = tbTraffic.Value + " vpm"; CalculateFuzzyLogic(); };

            // Defuzzified Output Label
            lblResult = new Label { Text = "Suggested Speed: -- km/h", Location = new Point(20, 260), AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.DarkGreen };

            // Diagnostics Log
            Label lblLogTitle = new Label { Text = "System Execution Trace:", Location = new Point(20, 310), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            rtbLog = new RichTextBox { Location = new Point(20, 335), Width = 510, Height = 450, ReadOnly = true, Font = new Font("Consolas", 9), BackColor = Color.White };

            // Graph: Distance (Input 1)
            Label lblDistGraph = new Label { Text = "Input: Distance Membership", Location = new Point(550, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            pbDistance = new PictureBox { Size = new Size(400, 200), Location = new Point(550, 45), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pbDistance.Paint += DrawDistanceGraph;

            // Graph: Traffic (Input 2)
            Label lblTrafGraph = new Label { Text = "Input: Traffic Membership", Location = new Point(550, 265), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            pbTraffic = new PictureBox { Size = new Size(400, 200), Location = new Point(550, 290), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pbTraffic.Paint += DrawTrafficGraph;

            // Graph: Output (Speed)
            Label lblOutGraph = new Label { Text = "Output: Speed Membership", Location = new Point(550, 510), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            pbOutput = new PictureBox { Size = new Size(400, 200), Location = new Point(550, 535), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pbOutput.Paint += DrawOutputGraph;

            // Add Controls
            this.Controls.Add(lblHeader);
            this.Controls.Add(lblDistanceTitle);
            this.Controls.Add(tbDistance);
            this.Controls.Add(lblDistanceValue);
            this.Controls.Add(lblTrafficTitle);
            this.Controls.Add(tbTraffic);
            this.Controls.Add(lblTrafficValue);
            this.Controls.Add(lblResult);
            this.Controls.Add(lblLogTitle);
            this.Controls.Add(rtbLog);

            this.Controls.Add(lblDistGraph);
            this.Controls.Add(pbDistance);
            this.Controls.Add(lblTrafGraph);
            this.Controls.Add(pbTraffic);
            this.Controls.Add(lblOutGraph);
            this.Controls.Add(pbOutput);

            CalculateFuzzyLogic();
        }

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

            // STEP A: FUZZIFICATION
            double dNear = Trap(dist, 0, 0, 200, 500);
            double dMed = Trap(dist, 200, 500, 500, 800);
            double dFar = Trap(dist, 500, 800, 1000, 1000);

            double tLight = Trap(traf, 0, 0, 30, 60);
            double tMod = Trap(traf, 30, 50, 50, 80);
            double tHeavy = Trap(traf, 50, 80, 100, 100);

            log.AppendLine($"=== Step A: Fuzzification ===");
            log.AppendLine($"Distance ({dist}m) -> Near: {dNear:F2} | Medium: {dMed:F2} | Far: {dFar:F2}");
            log.AppendLine($"Traffic  ({traf}vpm) -> Light: {tLight:F2} | Moderate: {tMod:F2} | Heavy: {tHeavy:F2}\n");

            // STEP B: RULE IMPLICATION
            double r1 = Math.Min(dNear, tLight);
            double r2 = Math.Min(dNear, tMod);
            double r3 = Math.Min(dNear, tHeavy);
            double r4 = Math.Min(dMed, tLight);
            double r5 = Math.Min(dMed, tMod);
            double r6 = Math.Min(dMed, tHeavy);
            double r7 = Math.Min(dFar, tLight);
            double r8 = Math.Min(dFar, tMod);
            double r9 = Math.Min(dFar, tHeavy);

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

            // STEP C: AGGREGATION
            double outSlow = Math.Max(r1, Math.Max(r2, Math.Max(r3, Math.Max(r6, r9))));
            double outCoast = Math.Max(r4, Math.Max(r5, r8));
            double outCruise = r7;

            log.AppendLine($"\n=== Step C: Aggregation (MAX) ===");
            log.AppendLine($"Aggregated Slow clipping height   : {outSlow:F2}");
            log.AppendLine($"Aggregated Coast clipping height  : {outCoast:F2}");
            log.AppendLine($"Aggregated Cruise clipping height : {outCruise:F2}\n");

            // STEP D: DEFUZZIFICATION 
            double numerator = 0;
            double denominator = 0;

            for (double v = 0; v <= 80; v += 1.0)
            {
                double mSlow = Trap(v, 0, 0, 0, 40);
                double mCoast = Trap(v, 20, 40, 40, 60);
                double mCruise = Trap(v, 40, 80, 80, 80);

                double cSlow = Math.Min(mSlow, outSlow);
                double cCoast = Math.Min(mCoast, outCoast);
                double cCruise = Math.Min(mCruise, outCruise);

                double cTotal = Math.Max(cSlow, Math.Max(cCoast, cCruise));
                numerator += v * cTotal;
                denominator += cTotal;
            }

            double finalSpeed = (denominator == 0) ? 0 : (numerator / denominator);

            log.AppendLine($"=== Step D: Defuzzification (Centroid) ===");
            log.AppendLine($"Numerator (Sum of x * y)   : {numerator:F2}");
            log.AppendLine($"Denominator (Area)         : {denominator:F2}");
            log.AppendLine($"Center of Gravity (Speed)  : {finalSpeed:F2} km/h");

            lblResult.Text = $"Suggested Speed: {finalSpeed:F1} km/h";
            rtbLog.Text = log.ToString();
            rtbLog.SelectionStart = 0;
            rtbLog.ScrollToCaret();

            currentCrispSpeed = finalSpeed;

            // Trigger visual redraws for all 3 graphs
            pbDistance.Invalidate();
            pbTraffic.Invalidate();
            pbOutput.Invalidate();
        }

        // Helper to dynamically scale values into graph pixels
        private PointF ScalePt(double val, double maxVal, double truth, int w, int h)
        {
            float px = (float)(val / maxVal * w);
            float py = (float)(h - 20 - (truth * (h - 40)));
            return new PointF(px, py);
        }

        private void DrawDistanceGraph(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int w = pbDistance.Width; int h = pbDistance.Height;
            g.DrawLine(Pens.Gray, 0, h - 20, w, h - 20);

            g.DrawLines(Pens.Blue, new[] { ScalePt(0, 1000, 1, w, h), ScalePt(200, 1000, 1, w, h), ScalePt(500, 1000, 0, w, h) });
            g.DrawString("Near", this.Font, Brushes.Blue, ScalePt(10, 1000, 1.05, w, h));

            g.DrawLines(Pens.Green, new[] { ScalePt(200, 1000, 0, w, h), ScalePt(500, 1000, 1, w, h), ScalePt(800, 1000, 0, w, h) });
            g.DrawString("Medium", this.Font, Brushes.Green, ScalePt(450, 1000, 1.05, w, h));

            g.DrawLines(Pens.Orange, new[] { ScalePt(500, 1000, 0, w, h), ScalePt(800, 1000, 1, w, h), ScalePt(1000, 1000, 1, w, h) });
            g.DrawString("Far", this.Font, Brushes.Orange, ScalePt(850, 1000, 1.05, w, h));

            float lineX = ScalePt(tbDistance.Value, 1000, 0, w, h).X;
            g.DrawLine(new Pen(Color.Red, 2), lineX, 0, lineX, h - 20);
            g.DrawString($"{tbDistance.Value}m", new Font(this.Font, FontStyle.Bold), Brushes.Red, lineX + 5, 10);
        }

        private void DrawTrafficGraph(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int w = pbTraffic.Width; int h = pbTraffic.Height;
            g.DrawLine(Pens.Gray, 0, h - 20, w, h - 20);

            g.DrawLines(Pens.Blue, new[] { ScalePt(0, 100, 1, w, h), ScalePt(30, 100, 1, w, h), ScalePt(60, 100, 0, w, h) });
            g.DrawString("Light", this.Font, Brushes.Blue, ScalePt(5, 100, 1.05, w, h));

            g.DrawLines(Pens.Green, new[] { ScalePt(30, 100, 0, w, h), ScalePt(50, 100, 1, w, h), ScalePt(80, 100, 0, w, h) });
            g.DrawString("Mod", this.Font, Brushes.Green, ScalePt(45, 100, 1.05, w, h));

            g.DrawLines(Pens.Orange, new[] { ScalePt(50, 100, 0, w, h), ScalePt(80, 100, 1, w, h), ScalePt(100, 100, 1, w, h) });
            g.DrawString("Heavy", this.Font, Brushes.Orange, ScalePt(82, 100, 1.05, w, h));

            float lineX = ScalePt(tbTraffic.Value, 100, 0, w, h).X;
            g.DrawLine(new Pen(Color.Red, 2), lineX, 0, lineX, h - 20);
            g.DrawString($"{tbTraffic.Value}vpm", new Font(this.Font, FontStyle.Bold), Brushes.Red, lineX + 5, 10);
        }

        private void DrawOutputGraph(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int w = pbOutput.Width; int h = pbOutput.Height;
            g.DrawLine(Pens.Gray, 0, h - 20, w, h - 20);

            g.DrawLines(Pens.Blue, new[] { ScalePt(0, 100, 1, w, h), ScalePt(20, 100, 1, w, h), ScalePt(40, 100, 0, w, h) });
            g.DrawString("Slow", this.Font, Brushes.Blue, ScalePt(5, 100, 1.05, w, h));

            g.DrawLines(Pens.Green, new[] { ScalePt(20, 100, 0, w, h), ScalePt(50, 100, 1, w, h), ScalePt(80, 100, 0, w, h) });
            g.DrawString("Coast", this.Font, Brushes.Green, ScalePt(40, 100, 1.05, w, h));

            g.DrawLines(Pens.Orange, new[] { ScalePt(60, 100, 0, w, h), ScalePt(80, 100, 1, w, h), ScalePt(100, 100, 1, w, h) });
            g.DrawString("Cruise", this.Font, Brushes.Orange, ScalePt(80, 100, 1.05, w, h));

            if (currentCrispSpeed > 0)
            {
                float lineX = ScalePt(currentCrispSpeed, 100, 0, w, h).X;
                g.DrawLine(new Pen(Color.Red, 2), lineX, 0, lineX, h - 20);
                g.DrawString($"Final: {currentCrispSpeed:F1} km/h", new Font(this.Font, FontStyle.Bold), Brushes.Red, lineX + 5, 10);
            }
        }
    }
}