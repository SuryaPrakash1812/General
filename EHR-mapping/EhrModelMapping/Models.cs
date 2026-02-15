namespace EhrModelMapping
{
    public class EhrFieldDto
    {
        public string FieldName { get; set; }
        public string DataType { get; set; }
        public bool Required { get; set; }
        public bool IsConsuming { get; set; }
        public string DefaultValue { get; set; }
        public string Transform { get; set; }
    }

    public class EhrModelConfigDto
    {
        public int InstitutionId { get; set; }
        public string ModelName { get; set; }
        public int ModelVersion { get; set; }
        public List<EhrFieldDto> Fields { get; set; }
        public int Id { get;  set; }
    }

    public class ModelSummaryDto
    {
        public string ModelName { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Id { get; internal set; }
    }

}
