using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;

namespace AutoUIPlayback
{
    class ElementList
    {
        private List<AutomationElement> elementList;

        public int Count
        {
            get { return elementList.Count; }
        }

        public ElementList()
        {
            elementList = new List<AutomationElement>();
        }

        public AutomationElement GetElement(int index)
        {
            return elementList[index];
        }

        public void Add(AutomationElement element)
        {
            elementList.Add(element);
        }

        public bool Equals(ElementList elements)
        {
            if (elementList.Count != elements.Count)
            {
                return false;
            }
            for (int i=0;i<elements.Count;i++)
            {
                if (this.GetElement(i) != elements.GetElement(i))
                {
                    return false;
                }
            }
            return true;
        }

        public void CopyFrom(ElementList elements)
        {
            for (int i = 0; i < elements.Count;i++ )
            {
                elementList.Add(elements.GetElement(i));
            }
        }

        public void CopyTo(ElementList elements)
        {
            for (int i = 0; i < elementList.Count;i++ )
            {
                elements.Add(this.GetElement(i));
            }
        }

        public void Clear()
        {
            elementList.Clear();
        }
    }
}
