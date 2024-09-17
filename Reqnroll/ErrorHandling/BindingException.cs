using Reqnroll.Bindings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

// the exceptions are part of the public API, keep them in Reqnroll namespace
namespace Reqnroll
{
    [Serializable]
    public class BindingException : ReqnrollException
    {
        public BindingException()
        {
        }

        public BindingException(string message) : base(message)
        {
        }

        public BindingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BindingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class AmbiguousBindingException : BindingException
    {
        public IEnumerable<BindingMatch> Matches { get; private set; }

        public AmbiguousBindingException()
        {
        }

        public AmbiguousBindingException(string message) : base(message)
        {
        }

        public AmbiguousBindingException(string message, Exception inner) : base(message, inner)
        {
        }

        public AmbiguousBindingException(string message, IEnumerable<BindingMatch> matches) : base(message)
        {
            Matches = new List<BindingMatch>(matches);
        }

        protected AmbiguousBindingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            Matches = (List<BindingMatch>)info.GetValue("Matches", typeof(List<BindingMatch>));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            base.GetObjectData(info, context);
            info.AddValue("Matches", Matches);
        }
    }
}