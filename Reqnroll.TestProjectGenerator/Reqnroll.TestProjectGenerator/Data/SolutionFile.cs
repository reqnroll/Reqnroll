using System;

namespace Reqnroll.TestProjectGenerator.Data
{
    public class SolutionFile(string path, string content)
    {
        private bool _isFrozen = false;

        public string Path { get; } = path; //relative from project
        public string Content { get; private set; } = content;

        internal void Freeze()
        {
            _isFrozen = true;
        }

        public void Append(string addedContent)
        {
            if (_isFrozen)
            {
                throw new InvalidOperationException("Cannot append to frozen file");
            }

            if (!Content.EndsWith(Environment.NewLine))
                Content += Environment.NewLine;

            Content += addedContent;
        }
    }
}