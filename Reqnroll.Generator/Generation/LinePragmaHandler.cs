using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Utils;
using System.CodeDom;
using System.IO;

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


        public void AddLinePragmaInitial(CodeTypeDeclaration testType, string sourceFile, string codeBehindFilePath)
        {
            if (_reqnrollConfiguration.AllowDebugGeneratedFiles)
                return;

            string sourceFileRelativePath = Path.GetFileName(sourceFile);

            if (codeBehindFilePath != null && Path.IsPathRooted(sourceFile))
            {
                var codeBehindFolder = Path.GetDirectoryName(codeBehindFilePath);
                if (codeBehindFolder != null)
                {
                    sourceFileRelativePath = FileSystemHelper.GetRelativePath(sourceFile, codeBehindFolder);
                }
            }

            _codeDomHelper.BindTypeToSourceFile(testType, sourceFileRelativePath);
        }
    }
}