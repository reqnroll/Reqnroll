using System;
using System.CodeDom;
using System.Collections.Generic;
using Gherkin.Ast;
using Reqnroll.Configuration;

namespace Reqnroll.Generator.CodeDom
{
    public class SourceLineScope : IDisposable
    {
        private readonly ReqnrollConfiguration _reqnrollConfiguration;
        private readonly CodeDomHelper _codeDomHelper;
        private readonly List<CodeStatement> _statements;
        private readonly Location _location;

        public SourceLineScope(ReqnrollConfiguration reqnrollConfiguration, CodeDomHelper codeDomHelper, List<CodeStatement> statements, string filename, Location location)
        {
            _reqnrollConfiguration = reqnrollConfiguration;
            _codeDomHelper = codeDomHelper;
            _statements = statements;
            _location = location;

            if (_location.Line <= 0 || _reqnrollConfiguration.AllowDebugGeneratedFiles)
            {
                return;
            }
                
            _statements.AddRange(_codeDomHelper.CreateSourceLinePragmaStatement(filename, _location.Line, _location.Column));
        }

        public void Dispose()
        {
            if (_location.Line <= 0 || _reqnrollConfiguration.AllowDebugGeneratedFiles)
            {
                return;
            }

            _statements.Add(_codeDomHelper.CreateDisableSourceLinePragmaStatement());
        }
    }
}