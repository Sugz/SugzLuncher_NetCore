using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SugzLuncher.Interfaces
{
    public interface ISerialize
    {
        public XElement Serialize();

        public void Deserialize(XElement xElement);

    }
}
