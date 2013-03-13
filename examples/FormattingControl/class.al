@# A template demostrating how to perfectly control the
@# output formatting.
@import System.Text
@param className: string
@param arrayLength: int
@param columns: string[]
@
namespace Cavingdeep.Sample
{
    public class @(className)
    {
        private int[] numbers = new int[] {@{
            for (int i = 1; i <= arrayLength; i++)
            {
                if (i < arrayLength)
                {
                    @text
                    @(i), @
                    @end_text
                }
                else
                {
                    @text
                    @(i)@
                    @end_text
                }
            }
        @}};

        // Fields
        @code
        foreach (string column in columns)
        {
            @text
            private string @(column);
            @end_text
        }
        @end_code

        public @(className)()
        {
        }

        // Properties
        @code
        for (int i = 0; i < columns.Length; i++)
        {
            string column = columns[i];

            @text
            @= Dcg.CallTemplate("property.al", null, null, column)
            @end_text

            if (i+1 < columns.Length)
            {
                Dcg.Write("\r\n");
            }
        }
        @end_code
    }
}