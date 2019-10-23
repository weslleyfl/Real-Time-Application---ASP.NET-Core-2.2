using System.IO.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;


namespace ASC.Utilities
{
    public class CompressaoGZipPipeline
    {
        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseResponseCompression();
        }
    }
}

/* Exemplo para uso
 * 
        [HttpGet("comprimir")]
        [MiddlewareFilter(typeof(CompressaoGZipPipeline))]
        public IActionResult GetComCompressao()
        {
            return GetJsonProdutos();
        }
     
*/
