using System.CodeDom;
using System.IO;
using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;

namespace Reqnroll.Generator.Generation
{
    public class LinePragmaHandler
    {
        private readonly ReqnrollConfiguration _reqnrollConfiguration;
        private readonly CodeDomHelper _codeDomHelper;

        public LinePragmaHandler(ReqnrollConfiguration reqnrollConfiguration, CodeDomHelper codeDomHelper)
        {
            _reqnrollConfiguration = reqnrollConfiguration;
            _codeDomHelper = codeDomHelper;
        }


        public void AddLinePragmaInitial(CodeTypeDeclaration testType, string sourceFile)
        {
            if (_reqnrollConfiguration.AllowDebugGeneratedFiles)
                return;

            _codeDomHelper.BindTypeToSourceFile(testType, Path.GetFileName(sourceFile));
        }
    }
}