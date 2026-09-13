namespace FraudAssessmentWebUI.Models
{
    public class FraudAssessmentViewModel
    {
        public string FileName { get; set; }
        public int RiskScore { get; set; }
        public bool TamperDetected { get; set; }
        public Dictionary<string, string> ExtractedFields { get; set; }
        public string Explanation { get; set; }
    }

}
