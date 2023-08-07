using System;
using System.Collections;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace wpf_sandbox
{
    public class PropertySpec
    {
        public object DefaultValue { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Category { get; set; }
        public bool Expandable { get; set; }
        public bool IsReadOnly { get; set; }
        public bool Browsable { get; set; }
        public bool Enabled { get; set; }
        public bool TriggersRefresh { get; set; }

        public PropertySpec(
            string name,
            Type type,
            string category,
            bool expandable = false,
            bool triggersRefresh = false,
            bool readOnly = false)
            : this(name, type.AssemblyQualifiedName, category, expandable, triggersRefresh, readOnly)
        { }

        public PropertySpec(
            string name,
            string typeName,
            string category,
            bool expandable = false,
            bool triggersRefresh = false,
            bool readOnly = false)
        {
            Name = name;
            TypeName = typeName;
            Category = category;
            Expandable = expandable;
            IsReadOnly = readOnly;
            TriggersRefresh = triggersRefresh;
            Browsable = true;
            Enabled = true;
            DefaultValue = null;
        }
    }

    public class PropertySpecEventArgs : EventArgs
    {
        public PropertySpec Property { get; private set; }
        public object Value { get; set; }

        public PropertySpecEventArgs(PropertySpec property, object value)
        {
            Property = property;
            Value = value;
        }
    }

    public delegate void PropertySpecEventHandler(object sender, PropertySpecEventArgs e);

    public class DynamicProperty : ICustomTypeDescriptor
    {
        #region PropertySpecCollection class definition
        [Serializable]
        public class PropertySpecCollection : IList
        {
            private ArrayList innerArray;

            public PropertySpecCollection()
            {
                innerArray = new ArrayList();
            }

            public int Count
            {
                get { return innerArray.Count; }
            }

            public bool IsFixedSize
            {
                get { return false; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return null; }
            }

            public PropertySpec this[int index]
            {
                get { return (PropertySpec)innerArray[index]; }
                set { innerArray[index] = value; }
            }

            public int Add(PropertySpec value)
            {
                int index = innerArray.Add(value);

                return index;
            }

            public void AddRange(PropertySpec[] array)
            {
                innerArray.AddRange(array);
            }

            public void Clear()
            {
                innerArray.Clear();
            }

            public bool Contains(PropertySpec item)
            {
                return innerArray.Contains(item);
            }

            public bool Contains(string name)
            {
                foreach (PropertySpec spec in innerArray)
                    if (spec.Name == name)
                        return true;

                return false;
            }

            public void CopyTo(PropertySpec[] array)
            {
                innerArray.CopyTo(array);
            }

            public void CopyTo(PropertySpec[] array, int index)
            {
                innerArray.CopyTo(array, index);
            }

            public IEnumerator GetEnumerator()
            {
                return innerArray.GetEnumerator();
            }

            public int IndexOf(PropertySpec value)
            {
                return innerArray.IndexOf(value);
            }

            public int IndexOf(string name)
            {
                int i = 0;

                foreach (PropertySpec spec in innerArray)
                {
                    if (spec.Name == name)
                        return i;

                    i++;
                }

                return -1;
            }

            public void Insert(int index, PropertySpec value)
            {
                innerArray.Insert(index, value);
            }

            public void Remove(PropertySpec obj)
            {
                innerArray.Remove(obj);
            }

            public void Remove(string name)
            {
                int index = IndexOf(name);
                RemoveAt(index);
            }

            public void RemoveAt(int index)
            {
                innerArray.RemoveAt(index);
            }

            public PropertySpec[] ToArray()
            {
                return (PropertySpec[])innerArray.ToArray(typeof(PropertySpec));
            }

            #region Explicit interface implementations for ICollection and IList
            void ICollection.CopyTo(Array array, int index)
            {
                CopyTo((PropertySpec[])array, index);
            }

            int IList.Add(object value)
            {
                return Add((PropertySpec)value);
            }

            bool IList.Contains(object obj)
            {
                return Contains((PropertySpec)obj);
            }

            object IList.this[int index]
            {
                get
                {
                    return ((PropertySpecCollection)this)[index];
                }
                set
                {
                    ((PropertySpecCollection)this)[index] = (PropertySpec)value;
                }
            }

            int IList.IndexOf(object obj)
            {
                return IndexOf((PropertySpec)obj);
            }

            void IList.Insert(int index, object value)
            {
                Insert(index, (PropertySpec)value);
            }

            void IList.Remove(object value)
            {
                Remove((PropertySpec)value);
            }
            #endregion
        }
        #endregion
        #region PropertySpecDescriptor class definition
        private class PropertySpecDescriptor : PropertyDescriptor
        {
            private DynamicProperty property;
            private PropertySpec item;

            public PropertySpecDescriptor(PropertySpec item, DynamicProperty property, string name, Attribute[] attrs) :
                base(name, attrs)
            {
                this.property = property;
                this.item = item;
            }

            public override Type ComponentType
            {
                get { return item.GetType(); }
            }

            public override bool IsReadOnly
            {
                get { return (Attributes.Matches(ReadOnlyAttribute.Yes)); }
            }

            public override Type PropertyType
            {
                get { return Type.GetType(item.TypeName); }
            }

            public override bool CanResetValue(object component)
            {
                if (item.DefaultValue == null)
                    return false;
                else
                    return !this.GetValue(component).Equals(item.DefaultValue);
            }

            public override object GetValue(object component)
            {
                PropertySpecEventArgs e = new PropertySpecEventArgs(item, null);
                property.OnGetValue(e);
                return e.Value;
            }

            public override void ResetValue(object component)
            {
                SetValue(component, item.DefaultValue);
            }

            public override void SetValue(object component, object value)
            {
                PropertySpecEventArgs e = new PropertySpecEventArgs(item, value);
                property.OnSetValue(e);
            }

            public override bool ShouldSerializeValue(object component)
            {
                object val = this.GetValue(component);

                if (item.DefaultValue == null && val == null)
                    return false;
                else
                    return !val.Equals(item.DefaultValue);
            }
        }
        #endregion

        public string DefaultProperty { get; set; }
        public PropertySpecCollection Properties { get; private set; }

        public DynamicProperty()
        {
            DefaultProperty = null;
            Properties = new PropertySpecCollection();
        }

        public override string ToString()
        {
            return "";
        }

        public event PropertySpecEventHandler GetValue;
        public event PropertySpecEventHandler SetValue;

        protected virtual void OnGetValue(PropertySpecEventArgs e)
        {
            if (GetValue != null)
                GetValue(this, e);
        }

        protected virtual void OnSetValue(PropertySpecEventArgs e)
        {
            if (SetValue != null)
                SetValue(this, e);
        }

        #region ICustomTypeDescriptor explicit interface definitions
        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            PropertySpec propertySpec = null;
            if (DefaultProperty != null)
            {
                int index = Properties.IndexOf(DefaultProperty);
                propertySpec = Properties[index];
            }

            if (propertySpec != null)
                return new PropertySpecDescriptor(propertySpec, this, propertySpec.Name, null);
            else
                return null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            ArrayList props = new ArrayList();

            int order = 0;

            foreach (PropertySpec property in Properties)
            {
                ArrayList attrs = new ArrayList();

                if (property.Category != null)
                    attrs.Add(new CategoryAttribute(property.Category));

                if (property.Expandable)
                    attrs.Add(new ExpandableObjectAttribute());

                if (property.IsReadOnly)
                    attrs.Add(ReadOnlyAttribute.Yes);

                if (property.Enabled)
                {
                    if (property.Browsable)
                        attrs.Add(BrowsableAttribute.Yes);
                    else
                        attrs.Add(BrowsableAttribute.No);
                }
                else
                    attrs.Add(BrowsableAttribute.No);

                if (property.TriggersRefresh)
                    attrs.Add(RefreshPropertiesAttribute.All);

                attrs.Add(new PropertyOrderAttribute(order));
                order++;

                Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

                PropertySpecDescriptor pd = new PropertySpecDescriptor(property, this, property.Name, attrArray);
                props.Add(pd);
            }

            PropertyDescriptor[] propArray = (PropertyDescriptor[])props.ToArray(typeof(PropertyDescriptor));
            return new PropertyDescriptorCollection(propArray);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
        #endregion
    }
}
