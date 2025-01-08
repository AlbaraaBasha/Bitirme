using Microsoft.ML.Data;

namespace Bitirme
{
    public class RetinopathyInput
    {
        [ColumnName("ImagePath")]  
        public string ImagePath { get; set; }  
    }

    public class RetinopathyOutput
    {
        [ColumnName("dense_11")]  
        public float[] Predictions { get; set; }
    }
}
