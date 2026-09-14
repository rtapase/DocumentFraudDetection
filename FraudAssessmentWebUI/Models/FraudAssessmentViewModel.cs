namespace FraudAssessmentWebUI.Models
{
    public class FraudAssessmentViewModel
    {
        public string FileName { get; set; }
        public string Author { get; set; }
        public string Producer { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int RiskScore { get; set; }
        public bool TamperDetected { get; set; }
        public Dictionary<string, string> ExtractedFields { get; set; }
        public string Explanation { get; set; }
    }

}
