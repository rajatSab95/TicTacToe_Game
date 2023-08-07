using System;
using System.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;

namespace wpf_sandbox
{
    using System;

    static class PropertyUtilities
    {
        public static void SetBrowsable(
            DynamicProperty.PropertySpecCollection properties,
            bool isBrowsable)
        {
            Debug.Assert(properties[0].Name == "Set");
            for (int i = 1; i < properties.Count; ++i)
                properties[i].Browsable = isBrowsable;
        }

        public static void EnableValidProperties(
            DynamicProperty.PropertySpecCollection properties,
            bool isEnabled)
        {
            int startingIndex = 1;
            if (properties[0].Name == "Set")
                startingIndex = 2;
            for (int i = startingIndex; i < properties.Count; ++i)
                properties[i].Enabled = isEnabled;
        }

        public static T Clamp<T>(
            T value,
            T min,
            T max)
            where T : IComparable
        {
            if (value.CompareTo(min) < 0)
                return min;
            else if (value.CompareTo(max) > 0)
                return max;
            else
                return value;
        }

        public static void Resize<T>(
            ref T[] array,
            int newSize,
            T newValue)
        {
            int oldSize = array.Length;
            Array.Resize(ref array, newSize);
            for (int i = oldSize; i < array.Length; ++i)
                array[i] = newValue;
        }
    }

    interface IRootProperty
    {
        void Apply();
    }

    public class NestedProperty : DynamicProperty
    {
        protected NestedProperty owner;

        public NestedProperty(
            NestedProperty owner)
        {
            this.owner = owner;
        }

        protected virtual void OnChildChanged()
        {
            if (this.owner != null)
                this.owner.OnChildChanged();
        }
    }

    #region ArrayProperty and friends
    [ExpandableObject]
    public abstract class ArrayProperty : NestedProperty
    {
        protected string itemPrefix;

        public ArrayProperty(
            NestedProperty owner,
            string itemPrefix)
            : base(owner)
        {
            this.itemPrefix = itemPrefix;
        }

        protected virtual int GetIndexFromName(
            PropertySpecEventArgs e)
        {
            int indexOffset = 0;
            if (itemPrefix.Length > 0)
                indexOffset = itemPrefix.Length + 1;
            string indexString = e.Property.Name.Substring(indexOffset);
            return Int32.Parse(indexString);
        }

        protected abstract void ResizeArrays(
            int newCount);

        protected abstract void AddProperties(
            int newCount);

        protected virtual bool AddOrDeleteItems(
            int oldCount,
            int newCount)
        {
            if (newCount == oldCount)
                return false;

            ResizeArrays(newCount);

            DeleteProperties(0, oldCount - 1);
            AddProperties(newCount);

            return true;
        }

        protected virtual void DeleteProperties(
            int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; --i)
                Properties.RemoveAt(i + 1);
        }
    }

    [ExpandableObject]
    public class ArrayColorProperty : NestedProperty
    {
        private int index;
        private HPS.RGBColor[] colors;

        public ArrayColorProperty(
            NestedProperty owner,
            int index,
            HPS.RGBColor[] colors)
            : base(owner)
        {
            this.index = index;
            this.colors = colors;

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Red", typeof(float), null, expandable: false));
            Properties.Add(new PropertySpec("Green", typeof(float), null, expandable: false));
            Properties.Add(new PropertySpec("Blue", typeof(float), null, expandable: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Red": e.Value = colors[index].red; break;
                case "Green": e.Value = colors[index].green; break;
                case "Blue": e.Value = colors[index].blue; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Red":
                    {
                        float newValue = PropertyUtilities.Clamp((float)e.Value, 0, 1);
                        colors[index].red = newValue;
                    }
                    break;
                case "Green":
                    {
                        float newValue = PropertyUtilities.Clamp((float)e.Value, 0, 1);
                        colors[index].green = newValue;
                    }
                    break;
                case "Blue":
                    {
                        float newValue = PropertyUtilities.Clamp((float)e.Value, 0, 1);
                        colors[index].blue = newValue;
                    }
                    break;
            }
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class ArrayPointProperty : NestedProperty
    {
        private int index;
        private HPS.Point[] points;

        public ArrayPointProperty(
            NestedProperty owner,
            int index,
            HPS.Point[] points)
            : base(owner)
        {
            this.index = index;
            this.points = points;

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("X", typeof(float), null, expandable: false));
            Properties.Add(new PropertySpec("Y", typeof(float), null, expandable: false));
            Properties.Add(new PropertySpec("Z", typeof(float), null, expandable: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "X": e.Value = points[index].x; break;
                case "Y": e.Value = points[index].y; break;
                case "Z": e.Value = points[index].z; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "X":
                    {
                        points[index].x = (float)e.Value;
                    }
                    break;
                case "Y":
                    {
                        points[index].y = (float)e.Value;
                    }
                    break;
                case "Z":
                    {
                        points[index].z = (float)e.Value;
                    }
                    break;
            }
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class ArrayPlaneProperty : NestedProperty
    {
        private int index;
        private HPS.Plane[] planes;

        public ArrayPlaneProperty(
            NestedProperty owner,
            int index,
            HPS.Plane[] planes)
            : base(owner)
        {
            this.index = index;
            this.planes = planes;

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("A", typeof(float), null, expandable: false));
            Properties.Add(new PropertySpec("B", typeof(float), null, expandable: false));
            Properties.Add(new PropertySpec("C", typeof(float), null, expandable: false));
            Properties.Add(new PropertySpec("D", typeof(float), null, expandable: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "A": e.Value = planes[index].a; break;
                case "B": e.Value = planes[index].b; break;
                case "C": e.Value = planes[index].c; break;
                case "D": e.Value = planes[index].d; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "A":
                    {
                        planes[index].a = (float)e.Value;
                    }
                    break;
                case "B":
                    {
                        planes[index].b = (float)e.Value;
                    }
                    break;
                case "C":
                    {
                        planes[index].c = (float)e.Value;
                    }
                    break;
                case "D":
                    {
                        planes[index].d = (float)e.Value;
                    }
                    break;
            }
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class ArrayFloatUnitProperty<T> : NestedProperty
    {
        private string floatName;
        private string unitName;
        private int index;
        private float[] floats;
        private T[] units;

        public ArrayFloatUnitProperty(
            NestedProperty owner,
            string floatName,
            string unitName,
            int index,
            float[] floats,
            T[] units)
            : base(owner)
        {
            this.floatName = floatName;
            this.unitName = unitName;
            this.index = index;
            this.floats = floats;
            this.units = units;

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec(this.floatName, typeof(float), null, expandable: false));
            Properties.Add(new PropertySpec(this.unitName, typeof(T), null, expandable: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            if (e.Property.Name == this.floatName)
                e.Value = floats[index];
            else if (e.Property.Name == this.unitName)
                e.Value = units[index];
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            if (e.Property.Name == this.floatName)
                floats[index] = (float)e.Value;
            else if (e.Property.Name == this.unitName)
                units[index] = (T)e.Value;
            OnChildChanged();
        }
    }
    #endregion

    #region Structs
    [ExpandableObject]
    public class RGBAColorProperty
    {
        public delegate void UpdateColor(HPS.RGBAColor color);

        private HPS.RGBAColor color;
        private UpdateColor updateColor;

        public RGBAColorProperty(
            HPS.RGBAColor color,
            UpdateColor updateColor)
        {
            this.color = color;
            this.updateColor = updateColor;
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public float Red
        {
            get { return color.red; }
            set
            {
                color.red = PropertyUtilities.Clamp(value, 0, 1);
                updateColor(color);
            }
        }

        [PropertyOrder(1)]
        public float Green
        {
            get { return color.green; }
            set
            {
                color.green = PropertyUtilities.Clamp(value, 0, 1);
                updateColor(color);
            }
        }

        [PropertyOrder(2)]
        public float Blue
        {
            get { return color.blue; }
            set
            {
                color.blue = PropertyUtilities.Clamp(value, 0, 1);
                updateColor(color);
            }
        }

        [PropertyOrder(3)]
        public float Alpha
        {
            get { return color.alpha; }
            set
            {
                color.alpha = PropertyUtilities.Clamp(value, 0, 1);
                updateColor(color);
            }
        }
    }

    [ExpandableObject]
    public class RGBColorProperty
    {
        public delegate void UpdateColor(HPS.RGBColor color);

        private HPS.RGBColor color;
        private UpdateColor updateColor;

        public RGBColorProperty(
            HPS.RGBColor color,
            UpdateColor updateColor)
        {
            this.color = color;
            this.updateColor = updateColor;
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public float Red
        {
            get { return color.red; }
            set
            {
                color.red = PropertyUtilities.Clamp(value, 0, 1);
                updateColor(color);
            }
        }

        [PropertyOrder(1)]
        public float Green
        {
            get { return color.green; }
            set
            {
                color.green = PropertyUtilities.Clamp(value, 0, 1);
                updateColor(color);
            }
        }

        [PropertyOrder(2)]
        public float Blue
        {
            get { return color.blue; }
            set
            {
                color.blue = PropertyUtilities.Clamp(value, 0, 1);
                updateColor(color);
            }
        }
    }

    [ExpandableObject]
    public class PointProperty
    {
        public delegate void UpdatePoint(HPS.Point point);

        private HPS.Point point;
        private UpdatePoint updatePoint;

        public PointProperty(
            HPS.Point point,
            UpdatePoint updatePoint)
        {
            this.point = point;
            this.updatePoint = updatePoint;
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public float X
        {
            get { return point.x; }
            set
            {
                point.x = value;
                updatePoint(point);
            }
        }

        [PropertyOrder(1)]
        public float Y
        {
            get { return point.y; }
            set
            {
                point.y = value;
                updatePoint(point);
            }
        }

        [PropertyOrder(2)]
        public float Z
        {
            get { return point.z; }
            set
            {
                point.z = value;
                updatePoint(point);
            }
        }
    }

    [ExpandableObject]
    public class GlyphPointProperty
    {
        public delegate void UpdatePoint(HPS.GlyphPoint point);

        private HPS.GlyphPoint point;
        private UpdatePoint updatePoint;

        public GlyphPointProperty(
            HPS.GlyphPoint point,
            UpdatePoint updatePoint)
        {
            this.point = point;
            this.updatePoint = updatePoint;
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public sbyte X
        {
            get { return point.x; }
            set
            {
                point.x = value;
                updatePoint(point);
            }
        }

        [PropertyOrder(1)]
        public sbyte Y
        {
            get { return point.y; }
            set
            {
                point.y = value;
                updatePoint(point);
            }
        }
    }

    [ExpandableObject]
    public class VectorProperty
    {
        public delegate void UpdateVector(HPS.Vector vector);

        private HPS.Vector vector;
        private UpdateVector updateVector;

        public VectorProperty(
            HPS.Vector vector,
            UpdateVector updateVector)
        {
            this.vector = vector;
            this.updateVector = updateVector;
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public float X
        {
            get { return vector.x; }
            set
            {
                vector.x = value;
                updateVector(vector);
            }
        }

        [PropertyOrder(1)]
        public float Y
        {
            get { return vector.y; }
            set
            {
                vector.y = value;
                updateVector(vector);
            }
        }

        [PropertyOrder(2)]
        public float Z
        {
            get { return vector.z; }
            set
            {
                vector.z = value;
                updateVector(vector);
            }
        }
    }

    [ExpandableObject]
    public class PlaneProperty
    {
        public delegate void UpdatePlane(HPS.Plane plane);

        private HPS.Plane plane;
        private UpdatePlane updatePlane;

        public PlaneProperty(
            HPS.Plane plane,
            UpdatePlane updatePlane)
        {
            this.plane = plane;
            this.updatePlane = updatePlane;
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public float A
        {
            get { return plane.a; }
            set
            {
                plane.a = value;
                updatePlane(plane);
            }
        }

        [PropertyOrder(1)]
        public float B
        {
            get { return plane.b; }
            set
            {
                plane.b = value;
                updatePlane(plane);
            }
        }

        [PropertyOrder(2)]
        public float C
        {
            get { return plane.c; }
            set
            {
                plane.c = value;
                updatePlane(plane);
            }
        }

        [PropertyOrder(3)]
        public float D
        {
            get { return plane.d; }
            set
            {
                plane.d = value;
                updatePlane(plane);
            }
        }
    }

    [ExpandableObject]
    public class RectangleProperty
    {
        public delegate void UpdateRectangle(HPS.Rectangle rectangle);

        private HPS.Rectangle rectangle;
        private UpdateRectangle updateRectangle;

        public RectangleProperty(
            HPS.Rectangle rectangle,
            UpdateRectangle updateRectangle)
        {
            this.rectangle = rectangle;
            this.updateRectangle = updateRectangle;
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public float Left
        {
            get { return rectangle.left; }
            set
            {
                rectangle.left = value;
                updateRectangle(rectangle);
            }
        }

        [PropertyOrder(1)]
        public float Right
        {
            get { return rectangle.right; }
            set
            {
                rectangle.right = value;
                updateRectangle(rectangle);
            }
        }

        [PropertyOrder(2)]
        public float Bottom
        {
            get { return rectangle.bottom; }
            set
            {
                rectangle.bottom = value;
                updateRectangle(rectangle);
            }
        }

        [PropertyOrder(3)]
        public float Top
        {
            get { return rectangle.top; }
            set
            {
                rectangle.top = value;
                updateRectangle(rectangle);
            }
        }
    }

    [ExpandableObject]
    public class IntRectangleProperty
    {
        public delegate void UpdateIntRectangle(HPS.IntRectangle rectangle);

        private HPS.IntRectangle rectangle;
        private UpdateIntRectangle updateIntRectangle;

        public IntRectangleProperty(
            HPS.IntRectangle rectangle,
            UpdateIntRectangle updateIntRectangle)
        {
            this.rectangle = rectangle;
            this.updateIntRectangle = updateIntRectangle;
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public int Left
        {
            get { return rectangle.left; }
            set
            {
                rectangle.left = value;
                updateIntRectangle(rectangle);
            }
        }

        [PropertyOrder(1)]
        public int Right
        {
            get { return rectangle.right; }
            set
            {
                rectangle.right = value;
                updateIntRectangle(rectangle);
            }
        }

        [PropertyOrder(2)]
        public int Bottom
        {
            get { return rectangle.bottom; }
            set
            {
                rectangle.bottom = value;
                updateIntRectangle(rectangle);
            }
        }

        [PropertyOrder(3)]
        public int Top
        {
            get { return rectangle.top; }
            set
            {
                rectangle.top = value;
                updateIntRectangle(rectangle);
            }
        }
    }

    [ExpandableObject]
    public class SimpleSphereProperty : DynamicProperty
    {
        public delegate void UpdateSimpleSphere(HPS.SimpleSphere simpleSphere);

        private HPS.SimpleSphere simpleSphere;
        private UpdateSimpleSphere updateSimpleSphere;

        private PointProperty centerProperty;

        public SimpleSphereProperty(
            HPS.SimpleSphere simpleSphere,
            UpdateSimpleSphere updateSimpleSphere)
        {
            this.simpleSphere = simpleSphere;
            this.updateSimpleSphere = updateSimpleSphere;

            centerProperty = new PointProperty(simpleSphere.center, UpdateCenter);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Center", typeof(PointProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Radius", typeof(float), null));
        }

        public override string ToString()
        {
            return "";
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Center": e.Value = centerProperty; break;
                case "Radius": e.Value = simpleSphere.radius; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Radius":
                {
                    simpleSphere.radius = PropertyUtilities.Clamp((float)e.Value, 0, float.MaxValue);
                    updateSimpleSphere(simpleSphere);
                }
                break;
                case "Center":
                {
                    // nothing to do
                }
                break;
            }
        }

        private void UpdateCenter(
            HPS.Point center)
        {
            simpleSphere.center = center;
            updateSimpleSphere(simpleSphere);
        }
    }

    [ExpandableObject]
    public class SimpleCuboidProperty : DynamicProperty
    {
        public delegate void UpdateSimpleCuboid(HPS.SimpleCuboid simpleCuboid);

        private HPS.SimpleCuboid simpleCuboid;
        private UpdateSimpleCuboid updateSimpleCuboid;

        private PointProperty minProperty;
        private PointProperty maxProperty;

        public SimpleCuboidProperty(
            HPS.SimpleCuboid simpleCuboid,
            UpdateSimpleCuboid updateSimpleCuboid)
        {
            this.simpleCuboid = simpleCuboid;
            this.updateSimpleCuboid = updateSimpleCuboid;

            minProperty = new PointProperty(simpleCuboid.min, UpdateMin);
            maxProperty = new PointProperty(simpleCuboid.max, UpdateMax);

            GetValue += Get;
            Properties.Add(new PropertySpec("Min", typeof(PointProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Max", typeof(PointProperty), null, expandable: true));
        }

        public override string ToString()
        {
            return "";
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Min": e.Value = minProperty; break;
                case "Max": e.Value = maxProperty; break;
            }
        }

        private void UpdateMin(
            HPS.Point min)
        {
            simpleCuboid.min = min;
            updateSimpleCuboid(simpleCuboid);
        }

        private void UpdateMax(
            HPS.Point max)
        {
            simpleCuboid.max = max;
            updateSimpleCuboid(simpleCuboid);
        }
    }
    #endregion

    #region Immutable Structs
    [ExpandableObject]
    public class ImmutablePointProperty
    {
        private HPS.Point point;

        public ImmutablePointProperty(
            HPS.Point point)
        {
            this.point = point;
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public float X
        {
            get { return point.x; }
        }

        [PropertyOrder(1)]
        public float Y
        {
            get { return point.y; }
        }

        [PropertyOrder(2)]
        public float Z
        {
            get { return point.z; }
        }
    }

    [ExpandableObject]
    public class ImmutableSimpleSphereProperty : DynamicProperty
    {
        private HPS.SimpleSphere simpleSphere;

        private ImmutablePointProperty centerProperty;

        public ImmutableSimpleSphereProperty(
            HPS.SimpleSphere simpleSphere)
        {
            this.simpleSphere = simpleSphere;

            centerProperty = new ImmutablePointProperty(simpleSphere.center);

            GetValue += Get;
            Properties.Add(new PropertySpec("Center", typeof(ImmutablePointProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Radius", typeof(float), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        public override string ToString()
        {
            return "";
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Center": e.Value = centerProperty; break;
                case "Radius": e.Value = simpleSphere.radius; break;
            }
        }
    }

    [ExpandableObject]
    public class ImmutableSimpleCuboidProperty : DynamicProperty
    {
        private HPS.SimpleCuboid simpleCuboid;

        private ImmutablePointProperty minProperty;
        private ImmutablePointProperty maxProperty;

        public ImmutableSimpleCuboidProperty(
            HPS.SimpleCuboid simpleCuboid)
        {
            this.simpleCuboid = simpleCuboid;

            minProperty = new ImmutablePointProperty(simpleCuboid.min);
            maxProperty = new ImmutablePointProperty(simpleCuboid.max);

            GetValue += Get;
            Properties.Add(new PropertySpec("Min", typeof(PointProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Max", typeof(PointProperty), null, expandable: true));
        }

        public override string ToString()
        {
            return "";
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Min": e.Value = minProperty; break;
                case "Max": e.Value = maxProperty; break;
            }
        }
    }
    #endregion

    #region Matrices
    public class MatrixKitProperty : NestedProperty
    {
        private HPS.MatrixKit kit;
        private float[] elements;

        public MatrixKitProperty(
            NestedProperty owner,
            HPS.MatrixKit kit)
            : base(owner)
        {
            this.kit = kit;

            this.kit.ShowElements(out elements);

            GetValue += Get;
            SetValue += Set;
            for (int i = 0; i < elements.Length; ++i)
                Properties.Add(new PropertySpec(i.ToString(), typeof(float), null));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            int index = Int32.Parse(e.Property.Name);
            e.Value = elements[index];
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            int index = Int32.Parse(e.Property.Name);
            elements[index] = (float)e.Value;

            kit.SetElement((ulong)index, elements[index]);
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class KitMatrixProperty<T> : NestedProperty
    {
        public delegate bool ShowMatrix(out HPS.MatrixKit matrix);
        public delegate T SetMatrix(HPS.MatrixKit matrix);
        public delegate T UnsetMatrix();

        private SetMatrix setMatrix;
        private UnsetMatrix unsetMatrix;
        private bool set;
        private HPS.MatrixKit kit;

        private MatrixKitProperty matrixProperty;

        public KitMatrixProperty(
            ShowMatrix showMatrix,
            SetMatrix setMatrix,
            UnsetMatrix unsetMatrix)
            : base(null)
        {
            this.setMatrix = setMatrix;
            this.unsetMatrix = unsetMatrix;

            set = showMatrix(out kit);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            matrixProperty = new MatrixKitProperty(this, kit);
            Properties.Add(new PropertySpec("Elements", typeof(MatrixKitProperty), null, expandable: true));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Elements": e.Value = matrixProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                        UpdateKit();
                    }
                    break;

                case "Elements":
                    {
                        // nothing to do
                    }
                    break;
            }
        }

        private void UpdateKit()
        {
            if (set)
                setMatrix(kit);
            else
                unsetMatrix();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }
    #endregion

    #region CameraKit
    [ExpandableObject]
    public class CameraKitPositionProperty
    {
        private HPS.CameraKit kit;
        private HPS.Point position;

        public CameraKitPositionProperty(
            HPS.CameraKit kit)
        {
            this.kit = kit;
            this.kit.ShowPosition(out position);
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public float X
        {
            get { return position.x; }
            set
            {
                position.x = value;
                kit.SetPosition(position);
            }
        }

        [PropertyOrder(0)]
        public float Y
        {
            get { return position.y; }
            set
            {
                position.y = value;
                kit.SetPosition(position);
            }
        }

        [PropertyOrder(0)]
        public float Z
        {
            get { return position.z; }
            set
            {
                position.z = value;
                kit.SetPosition(position);
            }
        }
    }

    [ExpandableObject]
    public class CameraKitTargetProperty
    {
        private HPS.CameraKit kit;
        private HPS.Point target;

        public CameraKitTargetProperty(
            HPS.CameraKit kit)
        {
            this.kit = kit;
            this.kit.ShowTarget(out target);
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public float X
        {
            get { return target.x; }
            set
            {
                target.x = value;
                kit.SetTarget(target);
            }
        }

        [PropertyOrder(0)]
        public float Y
        {
            get { return target.y; }
            set
            {
                target.y = value;
                kit.SetTarget(target);
            }
        }

        [PropertyOrder(0)]
        public float Z
        {
            get { return target.z; }
            set
            {
                target.z = value;
                kit.SetTarget(target);
            }
        }
    }

    [ExpandableObject]
    public class CameraKitUpVectorProperty
    {
        private HPS.CameraKit kit;
        private HPS.Vector upVector;

        public CameraKitUpVectorProperty(
            HPS.CameraKit kit)
        {
            this.kit = kit;
            this.kit.ShowUpVector(out upVector);
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public float X
        {
            get { return upVector.x; }
            set
            {
                upVector.x = value;
                kit.SetUpVector(upVector);
            }
        }

        [PropertyOrder(0)]
        public float Y
        {
            get { return upVector.y; }
            set
            {
                upVector.y = value;
                kit.SetUpVector(upVector);
            }
        }

        [PropertyOrder(0)]
        public float Z
        {
            get { return upVector.z; }
            set
            {
                upVector.z = value;
                kit.SetUpVector(upVector);
            }
        }
    }

    [ExpandableObject]
    public class CameraKitProjectionProperty : NestedProperty
    {
        private HPS.CameraKit kit;
        private HPS.Camera.Projection _type;
        private float _oblique_y_skew;
        private float _oblique_x_skew;

        public CameraKitProjectionProperty(
            HPS.CameraKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowProjection(out _type, out _oblique_y_skew, out _oblique_x_skew);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Type", typeof(HPS.Camera.Projection), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Oblique Y Skew", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Oblique X Skew", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type": e.Value = _type; break;
                case "Oblique Y Skew": e.Value = _oblique_y_skew; break;
                case "Oblique X Skew": e.Value = _oblique_x_skew; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type":
                    {
                        _type = (HPS.Camera.Projection)e.Value;
                    }
                    break;
                case "Oblique Y Skew":
                    {
                        _oblique_y_skew = (float)e.Value;
                    }
                    break;
                case "Oblique X Skew":
                    {
                        _oblique_x_skew = (float)e.Value;
                    }
                    break;
            }
            kit.SetProjection(_type, _oblique_y_skew, _oblique_x_skew);
        }
    }

    [ExpandableObject]
    public class CameraKitFieldProperty : NestedProperty
    {
        private HPS.CameraKit kit;
        private float _width;
        private float _height;

        public CameraKitFieldProperty(
            HPS.CameraKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowField(out _width, out _height);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Width", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Height", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Width": e.Value = _width; break;
                case "Height": e.Value = _height; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Width":
                    {
                        float newValue = PropertyUtilities.Clamp((float)e.Value, 0, float.MaxValue);
                        _width = newValue;
                    }
                    break;
                case "Height":
                    {
                        float newValue = PropertyUtilities.Clamp((float)e.Value, 0, float.MaxValue);
                        _height = newValue;
                    }
                    break;
            }
            kit.SetField(_width, _height);
        }
    }

    [ExpandableObject]
    public class CameraKitNearLimitProperty : NestedProperty
    {
        private HPS.CameraKit kit;
        private float _near_limit;

        public CameraKitNearLimitProperty(
            HPS.CameraKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowNearLimit(out _near_limit);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Near Limit", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Near Limit": e.Value = _near_limit; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Near Limit":
                    {
                        _near_limit = (float)e.Value;
                    }
                    break;
            }
            kit.SetNearLimit(_near_limit);
        }
    }

    [DisplayName("Camera")]
    public class CameraKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.CameraKit kit;

        public CameraKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            if (!this.key.ShowCamera(out kit))
                kit = HPS.CameraKit.GetDefault();

            Position = new CameraKitPositionProperty(kit);
            Target = new CameraKitTargetProperty(kit);
            UpVector = new CameraKitUpVectorProperty(kit);
            Projection = new CameraKitProjectionProperty(kit);
            Field = new CameraKitFieldProperty(kit);
            NearLimit = new CameraKitNearLimitProperty(kit);
        }

        [PropertyOrder(0)]
        public CameraKitPositionProperty Position { get; set; }

        [PropertyOrder(1)]
        public CameraKitTargetProperty Target { get; set; }

        [PropertyOrder(2)]
        public CameraKitUpVectorProperty UpVector { get; set; }

        [PropertyOrder(3)]
        public CameraKitProjectionProperty Projection { get; set; }

        [PropertyOrder(4)]
        public CameraKitFieldProperty Field { get; set; }

        [PropertyOrder(5)]
        public CameraKitNearLimitProperty NearLimit { get; set; }

        public void Apply()
        {
            key.SetCamera(kit);
        }
    }
    #endregion

    #region MaterialMappingKit
    public enum ComplexMaterialType
    {
        MaterialKit,
        MaterialIndex
    }

    public enum SimpleMaterialType
    {
        RGBAColor,
        MaterialIndex
    }

    public enum DiffuseColorType
    {
        RGBColor,
        RGBAColor,
        Alpha
    }

    public enum DiffuseTextureType
    {
        None,
        Texture,
        ModulatedTexture
    }

    public enum EnvironmentTextureType
    {
        None,
        Texture,
        ModulatedTexture,
        CubeMap,
        ModulatedCubeMap
    }

    public enum ModulatedChannelType
    {
        Color,
        Texture,
        ModulatedTexture
    }

    public enum TransmissionTextureType
    {
        Texture,
        ModulatedTexture
    }

    [ExpandableObject]
    public class SimpleMaterialProperty<T> : DynamicProperty
    {
        public delegate bool ShowColor(out HPS.Material.Type type, out HPS.RGBAColor color, out float index);
        public delegate T SetColor(HPS.RGBAColor color);
        public delegate T SetColorByIndex(float index);
        public delegate T UnsetColor();

        private SetColor setColor;
        private SetColorByIndex setColorByIndex;
        private UnsetColor unsetColor;
        private bool set;
        private SimpleMaterialType type;
        private HPS.RGBAColor color;
        private float index;

        private RGBAColorProperty colorProperty;

        public SimpleMaterialProperty(
            ShowColor showColor,
            SetColor setColor,
            SetColorByIndex setColorByIndex,
            UnsetColor unsetColor)
        {
            this.setColor = setColor;
            this.setColorByIndex = setColorByIndex;
            this.unsetColor = unsetColor;

            HPS.Material.Type materialType;
            set = showColor(out materialType, out color, out index);
            if (set)
            {
                type = (SimpleMaterialType)Enum.Parse(typeof(SimpleMaterialType), materialType.ToString());
                if (type == SimpleMaterialType.RGBAColor)
                    index = 0;
                else if (type == SimpleMaterialType.MaterialIndex)
                    color = HPS.RGBAColor.Black();
            }
            else
            {
                type = SimpleMaterialType.RGBAColor;
                color = HPS.RGBAColor.Black();
                index = 0;
            }

            colorProperty = new RGBAColorProperty(color, UpdateColor);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Type", typeof(SimpleMaterialType), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Color", typeof(RGBAColorProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Index", typeof(float), null));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Type": e.Value = type; break;
                case "Color": e.Value = colorProperty; break;
                case "Index": e.Value = index; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            bool performUpdate = true;
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;

                case "Type":
                    {
                        type = (SimpleMaterialType)e.Value;
                        EnableValidProperties();
                    }
                    break;

                case "Index":
                    {
                        index = (float)e.Value;
                    }
                    break;

                case "Color":
                default:
                    {
                        performUpdate = false;
                    }
                    break;
            }

            if (performUpdate)
                UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Set = 0,
            Type = 1,
            Color = 2,
            Index = 3
        }

        private void EnableValidProperties()
        {
            if (type == SimpleMaterialType.RGBAColor)
            {
                Properties[(int)PropertyTypeIndex.Color].Enabled = true;
                Properties[(int)PropertyTypeIndex.Index].Enabled = false;
            }
            else if (type == SimpleMaterialType.MaterialIndex)
            {
                Properties[(int)PropertyTypeIndex.Color].Enabled = false;
                Properties[(int)PropertyTypeIndex.Index].Enabled = true;
            }
        }

        private void UpdateKit()
        {
            if (set)
            {
                if (type == SimpleMaterialType.RGBAColor)
                    setColor(color);
                else if (type == SimpleMaterialType.MaterialIndex)
                    setColorByIndex(index);
            }
            else
                unsetColor();
        }

        private void UpdateColor(
            HPS.RGBAColor color)
        {
            this.color = color;
            UpdateKit();
        }
    }

    public class DiffuseColorProperty : NestedProperty
    {
        private HPS.MaterialKit kit;
        private bool set;
        private DiffuseColorType type;
        private HPS.RGBColor rgbColor;
        private float alpha;

        public DiffuseColorProperty(
            NestedProperty owner,
            HPS.MaterialKit kit)
            : base(owner)
        {
            this.kit = kit;

            type = DiffuseColorType.Alpha;
            if (this.kit.ShowDiffuseColor(out rgbColor))
            {
                set = true;
                type = DiffuseColorType.RGBColor;
                alpha = 1;
            }
            if (this.kit.ShowDiffuseAlpha(out alpha))
            {
                set = true;
                if (type == DiffuseColorType.RGBColor)
                    type = DiffuseColorType.RGBAColor;
                else
                {
                    Debug.Assert(type == DiffuseColorType.Alpha);
                    rgbColor = HPS.RGBColor.Black();
                }
            }

            if (!set)
            {
                type = DiffuseColorType.RGBAColor;
                rgbColor = HPS.RGBColor.Black();
                alpha = 1;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Type", typeof(DiffuseColorType), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Red", typeof(float), null));
            Properties.Add(new PropertySpec("Green", typeof(float), null));
            Properties.Add(new PropertySpec("Blue", typeof(float), null));
            Properties.Add(new PropertySpec("Alpha", typeof(float), null));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Type": e.Value = type; break;
                case "Red": e.Value = rgbColor.red; break;
                case "Green": e.Value = rgbColor.green; break;
                case "Blue": e.Value = rgbColor.blue; break;
                case "Alpha": e.Value = alpha; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            bool performUpdate = true;
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;

                case "Type":
                    {
                        type = (DiffuseColorType)e.Value;
                        EnableValidProperties();
                    }
                    break;

                case "Red":
                    {
                        rgbColor.red = (float)e.Value;
                    }
                    break;

                case "Green":
                    {
                        rgbColor.green = (float)e.Value;
                    }
                    break;

                case "Blue":
                    {
                        rgbColor.blue = (float)e.Value;
                    }
                    break;

                case "Alpha":
                    {
                        alpha = (float)e.Value;
                    }
                    break;

                default:
                    {
                        performUpdate = false;
                    }
                    break;
            }

            if (performUpdate)
                UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Type = 1,
            Red = 2,
            Green = 3,
            Blue = 4,
            Alpha = 5
        }

        private void EnableValidProperties()
        {
            if (type == DiffuseColorType.RGBColor)
            {
                Properties[(int)PropertyTypeIndex.Red].Enabled = true;
                Properties[(int)PropertyTypeIndex.Green].Enabled = true;
                Properties[(int)PropertyTypeIndex.Blue].Enabled = true;
                Properties[(int)PropertyTypeIndex.Alpha].Enabled = false;
            }
            else if (type == DiffuseColorType.RGBAColor)
            {
                Properties[(int)PropertyTypeIndex.Red].Enabled = true;
                Properties[(int)PropertyTypeIndex.Green].Enabled = true;
                Properties[(int)PropertyTypeIndex.Blue].Enabled = true;
                Properties[(int)PropertyTypeIndex.Alpha].Enabled = true;
            }
            else if (type == DiffuseColorType.Alpha)
            {
                Properties[(int)PropertyTypeIndex.Red].Enabled = false;
                Properties[(int)PropertyTypeIndex.Green].Enabled = false;
                Properties[(int)PropertyTypeIndex.Blue].Enabled = false;
                Properties[(int)PropertyTypeIndex.Alpha].Enabled = true;
            }
        }

        private void UpdateKit()
        {
            if (set)
            {
                if (type == DiffuseColorType.RGBColor)
                    kit.UnsetDiffuseColor().SetDiffuseColor(rgbColor);
                else if (type == DiffuseColorType.RGBAColor)
                    kit.UnsetDiffuseColor().SetDiffuseColor(new HPS.RGBAColor(rgbColor, alpha));
                else if (type == DiffuseColorType.Alpha)
                    kit.UnsetDiffuseColor().SetDiffuseAlpha(alpha);
            }
            else
                kit.UnsetDiffuseColor();
            OnChildChanged();
        }
    }

    public class DiffuseTextureLayerProperty : NestedProperty
    {
        private int index;
        private DiffuseTextureType[] types;
        private string[] textures;
        private HPS.RGBAColor[] modulationColors;

        private RGBAColorProperty colorProperty;

        public DiffuseTextureLayerProperty(
            NestedProperty owner,
            int index,
            DiffuseTextureType[] types,
            string[] textures,
            HPS.RGBAColor[] modulationColors)
            : base(owner)
        {
            this.index = index;
            this.types = types;
            this.textures = textures;
            this.modulationColors = modulationColors;

            colorProperty = new RGBAColorProperty(modulationColors[index], UpdateColor);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Type", typeof(DiffuseTextureType), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Texture", typeof(string), null));
            Properties.Add(new PropertySpec("ModulationColor", typeof(RGBAColorProperty), null, expandable: true));
            EnableValidProperties();
        }

        private static DiffuseTextureType MaterialTypeToDiffuseTextureType(
            HPS.Material.Type type)
        {
            if (type == HPS.Material.Type.TextureName)
                return DiffuseTextureType.Texture;
            else if (type == HPS.Material.Type.ModulatedTexture)
                return DiffuseTextureType.ModulatedTexture;
            else
                return DiffuseTextureType.None;
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type": e.Value = types[index]; break;
                case "Texture": e.Value = textures[index]; break;
                case "ModulationColor": e.Value = colorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type":
                    {
                        types[index] = (DiffuseTextureType)e.Value;
                        EnableValidProperties();
                    }
                    break;

                case "Texture":
                    {
                        textures[index] = (string)e.Value;
                    }
                    break;
            }
            OnChildChanged();
        }

        private enum PropertyTypeIndex
        {
            Type = 0,
            Texture = 1,
            Color = 2
        }

        private void EnableValidProperties()
        {
            if (types[index] == DiffuseTextureType.Texture)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = true;
                Properties[(int)PropertyTypeIndex.Color].Enabled = false;
            }
            else if (types[index] == DiffuseTextureType.ModulatedTexture)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = true;
                Properties[(int)PropertyTypeIndex.Color].Enabled = true;
            }
            else if (types[index] == DiffuseTextureType.None)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = false;
                Properties[(int)PropertyTypeIndex.Color].Enabled = false;
            }
        }

        private void UpdateColor(
            HPS.RGBAColor color)
        {
            modulationColors[index] = color;
            OnChildChanged();
        }
    }

    public class DiffuseTextureLayersArrayProperty : ArrayProperty
    {
        private int count;
        private DiffuseTextureType[] types;
        private string[] textures;
        private HPS.RGBAColor[] modulationColors;

        private ArrayList layerProperties;

        public DiffuseTextureLayersArrayProperty(
            NestedProperty owner,
            DiffuseTextureType[] types,
            string[] textures,
            HPS.RGBAColor[] modulationColors)
            : base(owner, "Layer")
        {
            this.types = types;
            this.textures = textures;
            this.modulationColors = modulationColors;

            count = types.Length;
            layerProperties = new ArrayList();

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref types, newCount, DiffuseTextureType.Texture);
            PropertyUtilities.Resize(ref textures, newCount, "texture");
            PropertyUtilities.Resize(ref modulationColors, newCount, HPS.RGBAColor.Black());
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                layerProperties.Add(new DiffuseTextureLayerProperty(this, i, types, textures, modulationColors));
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(DiffuseTextureLayerProperty), null, expandable: true));
            }
        }

        protected override void DeleteProperties(
            int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; --i)
            {
                Properties.RemoveAt(i + 1);
                layerProperties.RemoveAt(i);
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = layerProperties[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, 8);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
            }
            OnChildChanged();
        }

        public DiffuseTextureType[] Types
        {
            get { return types; }
        }

        public string[] Textures
        {
            get { return textures; }
        }

        public HPS.RGBAColor[] ModulationColors
        {
            get { return modulationColors; }
        }
    }

    public class DiffuseTextureProperty : NestedProperty
    {
        private HPS.MaterialKit kit;
        private bool set;

        private DiffuseTextureLayersArrayProperty layersProperty;

        public DiffuseTextureProperty(
            NestedProperty owner,
            HPS.MaterialKit kit)
            : base(owner)
        {
            this.kit = kit;

            HPS.Material.Type[] tempTypes;
            string[] tempTextures;
            HPS.RGBAColor[] tempModulationColors;
            set = this.kit.ShowDiffuseTexture(out tempTypes, out tempModulationColors, out tempTextures);

            DiffuseTextureType[] types = null;
            string[] textures = null;
            HPS.RGBAColor[] modulationColors = null;
            if (set)
            {
                int count = tempTypes.Length;
                Array.Resize(ref types, count);
                Array.Resize(ref textures, count);
                Array.Resize(ref modulationColors, count);
                for (int layer = 0; layer < count; ++layer)
                {
                    types[layer] = MaterialTypeToDiffuseTextureType(tempTypes[layer]);
                    if (types[layer] == DiffuseTextureType.None)
                    {
                        textures[layer] = "texture";
                        modulationColors[layer] = HPS.RGBAColor.Black();
                    }
                    else if (types[layer] == DiffuseTextureType.Texture)
                    {
                        textures[layer] = tempTextures[layer];
                        modulationColors[layer] = HPS.RGBAColor.Black();
                    }
                    else if (types[layer] == DiffuseTextureType.ModulatedTexture)
                    {
                        textures[layer] = tempTextures[layer];
                        modulationColors[layer] = tempModulationColors[layer];
                    }
                }
            }
            else
            {
                types = new DiffuseTextureType[] { DiffuseTextureType.Texture };
                textures = new string[] { "texture" };
                modulationColors = new HPS.RGBAColor[] { HPS.RGBAColor.Black() };
            }

            layersProperty = new DiffuseTextureLayersArrayProperty(this, types, textures, modulationColors);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Layers", typeof(DiffuseTextureLayersArrayProperty), null, expandable: true));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private static DiffuseTextureType MaterialTypeToDiffuseTextureType(
            HPS.Material.Type type)
        {
            if (type == HPS.Material.Type.TextureName)
                return DiffuseTextureType.Texture;
            else if (type == HPS.Material.Type.ModulatedTexture)
                return DiffuseTextureType.ModulatedTexture;
            else
                return DiffuseTextureType.None;
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Layers": e.Value = layersProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
            }
            OnChildChanged();
        }

        private void UpdateKit()
        {
            if (set)
            {
                kit.UnsetDiffuseTexture();
                for (int layer = 0; layer < layersProperty.Types.Length; ++layer)
                {
                    if (layersProperty.Types[layer] == DiffuseTextureType.None)
                        continue;

                    if (layersProperty.Types[layer] == DiffuseTextureType.Texture)
                        kit.SetDiffuseTexture(layersProperty.Textures[layer], (ulong)layer);
                    else if (layersProperty.Types[layer] == DiffuseTextureType.ModulatedTexture)
                        kit.SetDiffuseTexture(layersProperty.Textures[layer], layersProperty.ModulationColors[layer], (ulong)layer);
                }
            }
            else
                kit.UnsetDiffuseTexture();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    public class EnvironmentTextureProperty : NestedProperty
    {
        private HPS.MaterialKit kit;
        private bool set;
        private EnvironmentTextureType type;
        private string texture;
        private string cubemap;
        private HPS.RGBAColor modulationColor;

        private RGBAColorProperty colorProperty;

        public EnvironmentTextureProperty(
            NestedProperty owner,
            HPS.MaterialKit kit)
            : base(owner)
        {
            this.kit = kit;

            HPS.Material.Type materialType;
            string textureOrCubemap;
            set = this.kit.ShowEnvironment(out materialType, out modulationColor, out textureOrCubemap);
            type = MaterialTypeToEnvironmentTextureType(materialType);
            if (set)
            {
                if (type == EnvironmentTextureType.None)
                {
                    texture = "texture";
                    cubemap = "cubemap";
                    modulationColor = HPS.RGBAColor.Black();
                }
                else if (type == EnvironmentTextureType.Texture || type == EnvironmentTextureType.ModulatedTexture)
                {
                    texture = textureOrCubemap;
                    cubemap = "cubemap";
                    if (type == EnvironmentTextureType.Texture)
                        modulationColor = HPS.RGBAColor.Black();
                }
                else if (type == EnvironmentTextureType.CubeMap || type == EnvironmentTextureType.ModulatedCubeMap)
                {
                    cubemap = textureOrCubemap;
                    texture = "texture";
                    if (type == EnvironmentTextureType.CubeMap)
                        modulationColor = HPS.RGBAColor.Black();
                }
            }
            else
            {
                type = EnvironmentTextureType.Texture;
                texture = "texture";
                cubemap = "cubemap";
                modulationColor = HPS.RGBAColor.Black();
            }

            colorProperty = new RGBAColorProperty(modulationColor, UpdateColor);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Type", typeof(EnvironmentTextureType), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Texture", typeof(string), null));
            Properties.Add(new PropertySpec("CubeMap", typeof(string), null));
            Properties.Add(new PropertySpec("ModulationColor", typeof(RGBAColorProperty), null, expandable: true));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private static EnvironmentTextureType MaterialTypeToEnvironmentTextureType(
            HPS.Material.Type type)
        {
            if (type == HPS.Material.Type.TextureName)
                return EnvironmentTextureType.Texture;
            else if (type == HPS.Material.Type.ModulatedTexture)
                return EnvironmentTextureType.ModulatedTexture;
            else if (type == HPS.Material.Type.CubeMapName)
                return EnvironmentTextureType.CubeMap;
            else if (type == HPS.Material.Type.ModulatedCubeMap)
                return EnvironmentTextureType.ModulatedCubeMap;
            else
                return EnvironmentTextureType.None;
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Type": e.Value = type; break;
                case "Texture": e.Value = texture; break;
                case "CubeMap": e.Value = cubemap; break;
                case "ModulationColor": e.Value = colorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            bool performUpdate = true;
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;

                case "Type":
                    {
                        type = (EnvironmentTextureType)e.Value;
                        EnableValidProperties();
                    }
                    break;

                case "Texture":
                    {
                        texture = (string)e.Value;
                    }
                    break;

                case "CubeMap":
                    {
                        cubemap = (string)e.Value;
                    }
                    break;

                case "ModulationColor":
                default:
                    {
                        performUpdate = false;
                    }
                    break;
            }

            if (performUpdate)
                UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Set = 0,
            Type = 1,
            Texture = 2,
            CubeMap = 3,
            Color = 4
        }

        private void EnableValidProperties()
        {
            if (type == EnvironmentTextureType.None)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = false;
                Properties[(int)PropertyTypeIndex.CubeMap].Enabled = false;
                Properties[(int)PropertyTypeIndex.Color].Enabled = false;
            }
            else if (type == EnvironmentTextureType.Texture)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = true;
                Properties[(int)PropertyTypeIndex.CubeMap].Enabled = false;
                Properties[(int)PropertyTypeIndex.Color].Enabled = false;
            }
            else if (type == EnvironmentTextureType.ModulatedTexture)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = true;
                Properties[(int)PropertyTypeIndex.CubeMap].Enabled = false;
                Properties[(int)PropertyTypeIndex.Color].Enabled = true;
            }
            else if (type == EnvironmentTextureType.CubeMap)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = false;
                Properties[(int)PropertyTypeIndex.CubeMap].Enabled = true;
                Properties[(int)PropertyTypeIndex.Color].Enabled = false;
            }
            else if (type == EnvironmentTextureType.ModulatedCubeMap)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = false;
                Properties[(int)PropertyTypeIndex.CubeMap].Enabled = true;
                Properties[(int)PropertyTypeIndex.Color].Enabled = true;
            }
        }

        private void UpdateKit()
        {
            if (set)
            {
                if (type == EnvironmentTextureType.None)
                    kit.SetEnvironmentTexture();
                else if (type == EnvironmentTextureType.Texture)
                    kit.SetEnvironmentTexture(texture);
                else if (type == EnvironmentTextureType.ModulatedTexture)
                    kit.SetEnvironmentTexture(texture, modulationColor);
                else if (type == EnvironmentTextureType.CubeMap)
                    kit.SetEnvironmentCubeMap(cubemap);
                else if (type == EnvironmentTextureType.ModulatedCubeMap)
                    kit.SetEnvironmentCubeMap(cubemap, modulationColor);
            }
            else
                kit.UnsetEnvironment();
            OnChildChanged();
        }

        private void UpdateColor(
            HPS.RGBAColor color)
        {
            modulationColor = color;
            UpdateKit();
        }
    }

    public class ModulatedChannelProperty : NestedProperty
    {
        public delegate bool ShowColor(out HPS.Material.Type type, out HPS.RGBAColor color, out string texture);
        public delegate HPS.MaterialKit SetColor(HPS.RGBAColor color);
        public delegate HPS.MaterialKit SetTexture(string texture);
        public delegate HPS.MaterialKit SetModulatedTexture(string texture, HPS.RGBAColor color);
        public delegate HPS.MaterialKit UnsetColor();

        private SetColor setColor;
        private SetTexture setTexture;
        private SetModulatedTexture setModulatedTexture;
        private UnsetColor unsetColor;
        private bool set;
        ModulatedChannelType type;
        private HPS.RGBAColor color;
        private string texture;

        private RGBAColorProperty colorProperty;

        public ModulatedChannelProperty(
            NestedProperty owner,
            ShowColor showColor,
            SetColor setColor,
            SetTexture setTexture,
            SetModulatedTexture setModulatedTexture,
            UnsetColor unsetColor)
            : base(owner)
        {
            this.setColor = setColor;
            this.setTexture = setTexture;
            this.setModulatedTexture = setModulatedTexture;
            this.unsetColor = unsetColor;

            HPS.Material.Type materialType;
            set = showColor(out materialType, out color, out texture);
            type = MaterialTypeToModulatedChannelType(materialType);
            if (set)
            {
                if (type == ModulatedChannelType.Color)
                    texture = "texture";
                else if (type == ModulatedChannelType.Texture)
                    color = HPS.RGBAColor.Black();
            }
            else
            {
                type = ModulatedChannelType.Color;
                color = HPS.RGBAColor.Black();
                texture = "texture";
            }

            colorProperty = new RGBAColorProperty(color, UpdateColor);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Type", typeof(ModulatedChannelType), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Texture", typeof(string), null));
            Properties.Add(new PropertySpec("Color", typeof(RGBAColorProperty), null, expandable: true));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private static ModulatedChannelType MaterialTypeToModulatedChannelType(
            HPS.Material.Type type)
        {
            if (type == HPS.Material.Type.TextureName)
                return ModulatedChannelType.Texture;
            else if (type == HPS.Material.Type.ModulatedTexture)
                return ModulatedChannelType.ModulatedTexture;
            else
                return ModulatedChannelType.Color;
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Type": e.Value = type; break;
                case "Texture": e.Value = texture; break;
                case "Color": e.Value = colorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            bool performUpdate = true;
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;

                case "Type":
                    {
                        type = (ModulatedChannelType)e.Value;
                        EnableValidProperties();
                    }
                    break;

                case "Texture":
                    {
                        texture = (string)e.Value;
                    }
                    break;

                case "Color":
                default:
                    {
                        performUpdate = false;
                    }
                    break;
            }

            if (performUpdate)
                UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Set = 0,
            Type = 1,
            Texture = 2,
            Color = 3
        }

        private void EnableValidProperties()
        {
            if (type == ModulatedChannelType.Color)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = false;
                Properties[(int)PropertyTypeIndex.Color].Enabled = true;
            }
            else if (type == ModulatedChannelType.Texture)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = true;
                Properties[(int)PropertyTypeIndex.Color].Enabled = false;
            }
            else if (type == ModulatedChannelType.ModulatedTexture)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = true;
                Properties[(int)PropertyTypeIndex.Color].Enabled = true;
            }
        }

        private void UpdateKit()
        {
            if (set)
            {
                if (type == ModulatedChannelType.Color)
                    setColor(color);
                else if (type == ModulatedChannelType.Texture)
                    setTexture(texture);
                else if (type == ModulatedChannelType.ModulatedTexture)
                    setModulatedTexture(texture, color);
            }
            else
                unsetColor();
            OnChildChanged();
        }

        private void UpdateColor(
            HPS.RGBAColor color)
        {
            this.color = color;
            UpdateKit();
        }
    }

    public class TransmissionTextureProperty : NestedProperty
    {
        HPS.MaterialKit kit;
        private bool set;
        TransmissionTextureType type;
        private HPS.RGBAColor color;
        private string texture;

        private RGBAColorProperty colorProperty;

        public TransmissionTextureProperty(
            NestedProperty owner,
            HPS.MaterialKit kit)
            : base(owner)
        {
            this.kit = kit;

            HPS.Material.Type materialType;
            set = this.kit.ShowTransmission(out materialType, out color, out texture);
            type = MaterialTypeToTransmissionTextureType(materialType);
            if (set)
            {
                if (type == TransmissionTextureType.Texture)
                    color = HPS.RGBAColor.Black();
            }
            else
            {
                type = TransmissionTextureType.Texture;
                color = HPS.RGBAColor.Black();
                texture = "texture";
            }

            colorProperty = new RGBAColorProperty(color, UpdateColor);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Type", typeof(TransmissionTextureType), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Texture", typeof(string), null));
            Properties.Add(new PropertySpec("Color", typeof(RGBAColorProperty), null, expandable: true));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private static TransmissionTextureType MaterialTypeToTransmissionTextureType(
            HPS.Material.Type type)
        {
            if (type == HPS.Material.Type.TextureName)
                return TransmissionTextureType.Texture;
            else
                return TransmissionTextureType.ModulatedTexture;
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Type": e.Value = type; break;
                case "Texture": e.Value = texture; break;
                case "Color": e.Value = colorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            bool performUpdate = true;
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;

                case "Type":
                    {
                        type = (TransmissionTextureType)e.Value;
                        EnableValidProperties();
                    }
                    break;

                case "Texture":
                    {
                        texture = (string)e.Value;
                    }
                    break;

                case "Color":
                default:
                    {
                        performUpdate = false;
                    }
                    break;
            }

            if (performUpdate)
                UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Set = 0,
            Type = 1,
            Texture = 2,
            Color = 3
        }

        private void EnableValidProperties()
        {
            if (type == TransmissionTextureType.Texture)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = true;
                Properties[(int)PropertyTypeIndex.Color].Enabled = false;
            }
            else if (type == TransmissionTextureType.ModulatedTexture)
            {
                Properties[(int)PropertyTypeIndex.Texture].Enabled = true;
                Properties[(int)PropertyTypeIndex.Color].Enabled = true;
            }
        }

        private void UpdateKit()
        {
            if (set)
            {
                if (type == TransmissionTextureType.Texture)
                    kit.SetTransmission(texture);
                else if (type == TransmissionTextureType.ModulatedTexture)
                    kit.SetTransmission(texture, color);
            }
            else
                kit.UnsetTransmission();
            OnChildChanged();
        }

        private void UpdateColor(
            HPS.RGBAColor color)
        {
            this.color = color;
            UpdateKit();
        }
    }

    public class BumpTextureProperty : NestedProperty
    {
        HPS.MaterialKit kit;
        private bool set;
        private string texture;

        public BumpTextureProperty(
            NestedProperty owner,
            HPS.MaterialKit kit)
            : base(owner)
        {
            this.kit = kit;

            set = this.kit.ShowBump(out texture);
            if (!set)
                texture = "texture";

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Texture", typeof(string), null));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Texture": e.Value = texture; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            bool performUpdate = true;
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;

                case "Texture":
                    {
                        texture = (string)e.Value;
                    }
                    break;

                default:
                    {
                        performUpdate = false;
                    }
                    break;
            }

            if (performUpdate)
                UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBump(texture);
            else
                kit.UnsetBump();
            OnChildChanged();
        }
    }

    public class GlossProperty : NestedProperty
    {
        HPS.MaterialKit kit;
        private bool set;
        private float gloss;

        public GlossProperty(
            NestedProperty owner,
            HPS.MaterialKit kit)
            : base(owner)
        {
            this.kit = kit;

            set = this.kit.ShowGloss(out gloss);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Gloss", typeof(float), null));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Gloss": e.Value = gloss; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            bool performUpdate = true;
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;

                case "Texture":
                    {
                        gloss = (float)e.Value;
                    }
                    break;

                default:
                    {
                        performUpdate = false;
                    }
                    break;
            }

            if (performUpdate)
                UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetGloss(gloss);
            else
                kit.UnsetGloss();
            OnChildChanged();
        }
    }

    public class LegacyShaderProperty : NestedProperty
    {
        HPS.MaterialKit kit;
        private bool set;
        private string shader;

        public LegacyShaderProperty(
            NestedProperty owner,
            HPS.MaterialKit kit)
            : base(owner)
        {
            this.kit = kit;

            set = this.kit.ShowLegacyShader(out shader);
            if (!set)
                shader = "shader";

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Name", typeof(string), null));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Name": e.Value = shader; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            bool performUpdate = true;
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;

                case "Name":
                    {
                        shader = (string)e.Value;
                    }
                    break;

                default:
                    {
                        performUpdate = false;
                    }
                    break;
            }

            if (performUpdate)
                UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetLegacyShader(shader);
            else
                kit.UnsetLegacyShader();
            OnChildChanged();
        }
    }

    public class MaterialKitProperty : NestedProperty
    {
        private DiffuseColorProperty diffuseColorProperty;
        private DiffuseTextureProperty diffuseTextureProperty;
        private EnvironmentTextureProperty environmentTextureProperty;
        private ModulatedChannelProperty specularProperty;
        private ModulatedChannelProperty mirrorProperty;
        private ModulatedChannelProperty emissionProperty;
        private TransmissionTextureProperty transmissionTextureProperty;
        private BumpTextureProperty bumpTextureProperty;
        private GlossProperty glossProperty;
        private LegacyShaderProperty legacyShaderProperty;

        public MaterialKitProperty(
            NestedProperty owner,
            HPS.MaterialKit kit)
            : base(owner)
        {
            diffuseColorProperty = new DiffuseColorProperty(this, kit);
            diffuseTextureProperty = new DiffuseTextureProperty(this, kit);
            environmentTextureProperty = new EnvironmentTextureProperty(this, kit);
            specularProperty = new ModulatedChannelProperty(this, kit.ShowSpecular, kit.SetSpecular, kit.SetSpecular, kit.SetSpecular, kit.UnsetSpecular);
            mirrorProperty = new ModulatedChannelProperty(this, kit.ShowMirror, kit.SetMirror, kit.SetMirror, kit.SetMirror, kit.UnsetMirror);
            emissionProperty = new ModulatedChannelProperty(this, kit.ShowEmission, kit.SetEmission, kit.SetEmission, kit.SetEmission, kit.UnsetEmission);
            transmissionTextureProperty = new TransmissionTextureProperty(this, kit);
            bumpTextureProperty = new BumpTextureProperty(this, kit);
            glossProperty = new GlossProperty(this, kit);
            legacyShaderProperty = new LegacyShaderProperty(this, kit);

            GetValue += Get;
            Properties.Add(new PropertySpec("DiffuseColor", typeof(DiffuseColorProperty), null, expandable: true));
            Properties.Add(new PropertySpec("DiffuseTexture", typeof(DiffuseTextureProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Environment", typeof(EnvironmentTextureProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Specular", typeof(ModulatedChannelProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Mirror", typeof(ModulatedChannelProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Emission", typeof(ModulatedChannelProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Transmission", typeof(TransmissionTextureProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Bump", typeof(BumpTextureProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Gloss", typeof(GlossProperty), null, expandable: true));
            Properties.Add(new PropertySpec("LegacyShader", typeof(LegacyShaderProperty), null, expandable: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "DiffuseColor": e.Value = diffuseColorProperty; break;
                case "DiffuseTexture": e.Value = diffuseTextureProperty; break;
                case "Environment": e.Value = environmentTextureProperty; break;
                case "Specular": e.Value = specularProperty; break;
                case "Mirror": e.Value = mirrorProperty; break;
                case "Emission": e.Value = emissionProperty; break;
                case "Transmission": e.Value = transmissionTextureProperty; break;
                case "Bump": e.Value = bumpTextureProperty; break;
                case "Gloss": e.Value = glossProperty; break;
                case "LegacyShader": e.Value = legacyShaderProperty; break;
            }
        }
    }

    [ExpandableObject]
    public class ComplexMaterialProperty : NestedProperty
    {
        public delegate bool ShowMaterial(out HPS.Material.Type type, out HPS.MaterialKit material, out float index);
        public delegate HPS.MaterialMappingKit SetMaterial(HPS.MaterialKit material);
        public delegate HPS.MaterialMappingKit SetMaterialByIndex(float index);
        public delegate HPS.MaterialMappingKit UnsetMaterial();

        private SetMaterial setMaterial;
        private SetMaterialByIndex setMaterialByIndex;
        private UnsetMaterial unsetMaterial;
        private bool set;
        private ComplexMaterialType type;
        private HPS.MaterialKit material;
        private float index;

        private MaterialKitProperty materialProperty;

        public ComplexMaterialProperty(
            ShowMaterial showMaterial,
            SetMaterial setMaterial,
            SetMaterialByIndex setMaterialByIndex,
            UnsetMaterial unsetMaterial)
            : base(null)
        {
            this.setMaterial = setMaterial;
            this.setMaterialByIndex = setMaterialByIndex;
            this.unsetMaterial = unsetMaterial;

            HPS.Material.Type materialType;
            set = showMaterial(out materialType, out material, out index);
            type = MaterialTypeToComplexMaterialType(materialType);

            materialProperty = new MaterialKitProperty(this, material);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Type", typeof(ComplexMaterialType), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Material", typeof(MaterialKitProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Index", typeof(float), null, expandable: false));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private static ComplexMaterialType MaterialTypeToComplexMaterialType(
            HPS.Material.Type materialType)
        {
            if (materialType == HPS.Material.Type.FullMaterial)
                return ComplexMaterialType.MaterialKit;
            else
                return ComplexMaterialType.MaterialIndex;
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Type": e.Value = type; break;
                case "Material": e.Value = materialProperty; break;
                case "Index": e.Value = index; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            bool performUpdate = true;
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;

                case "Type":
                    {
                        type = (ComplexMaterialType)e.Value;
                        EnableValidProperties();
                    }
                    break;

                case "Index":
                    {
                        index = (float)e.Value;
                    }
                    break;

                case "Material":
                default:
                    {
                        performUpdate = false;
                    }
                    break;
            }
            if (performUpdate)
                UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Type = 1,
            Material = 2,
            Index = 3
        }

        private void EnableValidProperties()
        {
            if (type == ComplexMaterialType.MaterialKit)
            {
                Properties[(int)PropertyTypeIndex.Material].Enabled = true;
                Properties[(int)PropertyTypeIndex.Index].Enabled = false;
            }
            else if (type == ComplexMaterialType.MaterialIndex)
            {
                Properties[(int)PropertyTypeIndex.Material].Enabled = false;
                Properties[(int)PropertyTypeIndex.Index].Enabled = true;
            }
        }

        private void UpdateKit()
        {
            if (set)
            {
                if (type == ComplexMaterialType.MaterialKit)
                    setMaterial(material);
                else if (type == ComplexMaterialType.MaterialIndex)
                    setMaterialByIndex(index);
            }
            else
                unsetMaterial();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    [DisplayName("Material")]
    public class MaterialMappingKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.MaterialMappingKit kit;

        public MaterialMappingKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowMaterialMapping(out kit);

            FrontFaceMaterial = new ComplexMaterialProperty(kit.ShowFrontFaceMaterial, kit.SetFrontFaceMaterial, kit.SetFrontFaceMaterialByIndex, kit.UnsetFrontFaceMaterial);
            BackFaceMaterial = new ComplexMaterialProperty(kit.ShowBackFaceMaterial, kit.SetBackFaceMaterial, kit.SetBackFaceMaterialByIndex, kit.UnsetBackFaceMaterial);
            EdgeMaterial = new ComplexMaterialProperty(kit.ShowEdgeMaterial, kit.SetEdgeMaterial, kit.SetEdgeMaterialByIndex, kit.UnsetEdgeMaterial);
            VertexMaterial = new ComplexMaterialProperty(kit.ShowVertexMaterial, kit.SetVertexMaterial, kit.SetVertexMaterialByIndex, kit.UnsetVertexMaterial);

            LineColor = new SimpleMaterialProperty<HPS.MaterialMappingKit>(kit.ShowLineColor, kit.SetLineColor, kit.SetLineMaterialByIndex, kit.UnsetLineColor);
            TextColor = new SimpleMaterialProperty<HPS.MaterialMappingKit>(kit.ShowTextColor, kit.SetTextColor, kit.SetTextMaterialByIndex, kit.UnsetTextColor);
            MarkerColor = new SimpleMaterialProperty<HPS.MaterialMappingKit>(kit.ShowMarkerColor, kit.SetMarkerColor, kit.SetMarkerMaterialByIndex, kit.UnsetMarkerColor);
            LightColor = new SimpleMaterialProperty<HPS.MaterialMappingKit>(kit.ShowLightColor, kit.SetLightColor, kit.SetLightMaterialByIndex, kit.UnsetLightColor);
            WindowColor = new SimpleMaterialProperty<HPS.MaterialMappingKit>(kit.ShowWindowColor, kit.SetWindowColor, kit.SetWindowMaterialByIndex, kit.UnsetWindowColor);
            WindowContrastColor = new SimpleMaterialProperty<HPS.MaterialMappingKit>(kit.ShowWindowContrastColor, kit.SetWindowContrastColor, kit.SetWindowContrastMaterialByIndex, kit.UnsetWindowContrastColor);
            AmbientLightUpColor = new SimpleMaterialProperty<HPS.MaterialMappingKit>(kit.ShowAmbientLightUpColor, kit.SetAmbientLightUpColor, kit.SetAmbientLightUpMaterialByIndex, kit.UnsetAmbientLightUpColor);
            AmbientLightDownColor = new SimpleMaterialProperty<HPS.MaterialMappingKit>(kit.ShowAmbientLightDownColor, kit.SetAmbientLightDownColor, kit.SetAmbientLightDownMaterialByIndex, kit.UnsetAmbientLightDownColor);

            CutFaceMaterial = new ComplexMaterialProperty(kit.ShowCutFaceMaterial, kit.SetCutFaceMaterial, kit.SetCutFaceMaterialByIndex, kit.UnsetCutFaceMaterial);
            CutEdgeColor = new SimpleMaterialProperty<HPS.MaterialMappingKit>(kit.ShowCutEdgeColor, kit.SetCutEdgeColor, kit.SetCutEdgeMaterialByIndex, kit.UnsetCutEdgeColor);
        }

        [PropertyOrder(0)]
        public ComplexMaterialProperty FrontFaceMaterial { get; set; }

        [PropertyOrder(1)]
        public ComplexMaterialProperty BackFaceMaterial { get; set; }

        [PropertyOrder(2)]
        public ComplexMaterialProperty EdgeMaterial { get; set; }

        [PropertyOrder(3)]
        public ComplexMaterialProperty VertexMaterial { get; set; }

        [PropertyOrder(4)]
        public SimpleMaterialProperty<HPS.MaterialMappingKit> LineColor { get; set; }

        [PropertyOrder(5)]
        public SimpleMaterialProperty<HPS.MaterialMappingKit> TextColor { get; set; }

        [PropertyOrder(6)]
        public SimpleMaterialProperty<HPS.MaterialMappingKit> MarkerColor { get; set; }

        [PropertyOrder(7)]
        public SimpleMaterialProperty<HPS.MaterialMappingKit> LightColor { get; set; }

        [PropertyOrder(8)]
        public SimpleMaterialProperty<HPS.MaterialMappingKit> WindowColor { get; set; }

        [PropertyOrder(9)]
        public SimpleMaterialProperty<HPS.MaterialMappingKit> WindowContrastColor { get; set; }

        [PropertyOrder(10)]
        public SimpleMaterialProperty<HPS.MaterialMappingKit> AmbientLightUpColor { get; set; }

        [PropertyOrder(11)]
        public SimpleMaterialProperty<HPS.MaterialMappingKit> AmbientLightDownColor { get; set; }

        [PropertyOrder(12)]
        public ComplexMaterialProperty CutFaceMaterial { get; set; }

        [PropertyOrder(13)]
        public SimpleMaterialProperty<HPS.MaterialMappingKit> CutEdgeColor { get; set; }

        public void Apply()
        {
            key.UnsetMaterialMapping();
            key.SetMaterialMapping(kit);
        }
    }
    #endregion

    #region Priority and UserData
    [ExpandableObject]
    public class PriorityProperty<T> : NestedProperty
    {
        public delegate bool ShowPriority(out int priority);
        public delegate T SetPriority(int priority);
        public delegate T UnsetPriority();

        private SetPriority setPriority;
        private UnsetPriority unsetPriority;
        private bool set;
        private int priority;

        public PriorityProperty(
            ShowPriority showPriority,
            SetPriority setPriority,
            UnsetPriority unsetPriority)
            : base(null)
        {
            this.setPriority = setPriority;
            this.unsetPriority = unsetPriority;

            set = showPriority(out priority);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Value", typeof(int), null));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Value": e.Value = priority; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;

                case "Value":
                {
                    priority = (int)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                setPriority(priority);
            else
                unsetPriority();
        }
    }

    [ExpandableObject]
    public class SingleUserDataProperty
    {
        public SingleUserDataProperty(
            IntPtr index,
            int byteCount)
        {
            Index = index;
            ByteCount = byteCount;
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public IntPtr Index { get; private set; }

        [PropertyOrder(1)]
        public int ByteCount { get; private set; }
    }

    public class UserDataImmutableArrayProperty : ArrayProperty
    {
        private int count;
        private IntPtr[] indices;
        private byte[][] data;

        private ArrayList dataProperties;

        public UserDataImmutableArrayProperty(
            NestedProperty owner,
            IntPtr[] indices,
            byte[][] data)
            : base(owner, "Data")
        {
            this.indices = indices;
            this.data = data;

            count = this.indices.Length;
            dataProperties = new ArrayList();

            GetValue += Get;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: false, readOnly: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            // Unused
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                dataProperties.Add(new SingleUserDataProperty(indices[i], data[i].Length));
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(SingleUserDataProperty), null, expandable: true, triggersRefresh: false, readOnly: true));
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = dataProperties[GetIndexFromName(e)];
                    }
                    break;
            }
        }
    }

    [ExpandableObject]
    public class UserDataProperty<T> : NestedProperty
    {
        public delegate bool ShowUserData(out IntPtr[] indices, out byte[][] data);
        public delegate T SetUserData(IntPtr[] indices, byte[][] data);
        public delegate T UnsetAllUserData();

        private SetUserData setUserData;
        private UnsetAllUserData unsetAllUserData;
        private bool set;
        private IntPtr[] indices;
        private byte[][] data;

        private UserDataImmutableArrayProperty userDataProperty;

        public UserDataProperty(
            ShowUserData showUserData,
            SetUserData setUserData,
            UnsetAllUserData unsetAllUserData)
            : base(null)
        {
            this.setUserData = setUserData;
            this.unsetAllUserData = unsetAllUserData;

            set = showUserData(out indices, out data);
            if (set)
                userDataProperty = new UserDataImmutableArrayProperty(this, indices, data);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            if (set)
                Properties.Add(new PropertySpec("User Data", typeof(UserDataImmutableArrayProperty), null, expandable: true));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "User Data": e.Value = userDataProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                    UpdateKit();
                }
                break;
            }
        }

        private void UpdateKit()
        {
            if (set)
                setUserData(indices, data);
            else
                unsetAllUserData();
        }
    }
    #endregion

    #region ShellKit
    [ExpandableObject]
    public class GeometryPointCountProperty : NestedProperty
    {
        public delegate ulong GetPointCount();

        private ulong pointCount;

        public GeometryPointCountProperty(
            GetPointCount getPointCount)
            : base(null)
        {
            pointCount = getPointCount();

            GetValue += Get;
            Properties.Add(new PropertySpec("Count", typeof(ulong), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = pointCount; break;
            }
        }
    }

    [ExpandableObject]
    public class ShellKitFacelistProperty : NestedProperty
    {
        private int facelistSize;
        private ulong faceCount;

        public ShellKitFacelistProperty(
            HPS.ShellKit kit)
            : base(null)
        {
            int[] facelist;
            kit.ShowFacelist(out facelist);

            facelistSize = facelist.Length;
            faceCount = kit.GetFaceCount();

            GetValue += Get;
            Properties.Add(new PropertySpec("Facelist Size", typeof(int), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Face Count", typeof(ulong), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Facelist Size": e.Value = facelistSize; break;
                case "Face Count": e.Value = faceCount; break;
            }
        }
    }

    static class VertexUtilities
    {
        public delegate bool ShowVertexColors<T>(T component, out HPS.Material.Type[] types, out HPS.RGBColor[] rgbColors, out HPS.RGBAColor[] rgbaColors, out float[] indices);

        public static string GetVertexColorQuantity<T>(
            ShowVertexColors<T> showVertexColors,
            T component)
        {
            HPS.Material.Type[] types;
            HPS.RGBColor[] rgbColors;
            HPS.RGBAColor[] rgbaColors;
            float[] indices;
            if (showVertexColors(component, out types, out rgbColors, out rgbaColors, out indices))
            {
                if (Array.IndexOf(types, HPS.Material.Type.None) == -1)
                    return "All";
                else
                    return "Some";
            }
            else
                return "None";
        }
    }

    [ExpandableObject]
    public class ShellKitVertexColorsProperty : NestedProperty
    {
        private string faceQuantity;
        private string edgeQuantity;
        private string vertexQuantity;

        public ShellKitVertexColorsProperty(
            HPS.ShellKit kit)
            : base(null)
        {
            faceQuantity = VertexUtilities.GetVertexColorQuantity<HPS.Shell.Component>(kit.ShowVertexColors, HPS.Shell.Component.Faces);
            edgeQuantity = VertexUtilities.GetVertexColorQuantity<HPS.Shell.Component>(kit.ShowVertexColors, HPS.Shell.Component.Edges);
            vertexQuantity = VertexUtilities.GetVertexColorQuantity<HPS.Shell.Component>(kit.ShowVertexColors, HPS.Shell.Component.Vertices);

            GetValue += Get;
            Properties.Add(new PropertySpec("Faces", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Edges", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Vertices", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Faces": e.Value = faceQuantity; break;
                case "Edges": e.Value = edgeQuantity; break;
                case "Vertices": e.Value = vertexQuantity; break;
            }
        }
    }

    [ExpandableObject]
    public class PolyhedronNormalsProperty : NestedProperty
    {
        public delegate bool ShowNormals(out bool[] validities, out HPS.Vector[] normals);

        private string quantity;

        public PolyhedronNormalsProperty(
            ShowNormals showNormals)
            : base(null)
        {
            bool[] _validities;
            HPS.Vector[] _normals;
            if (showNormals(out _validities, out _normals))
            {
                if (Array.IndexOf(_validities, false) == -1)
                    quantity = "All";
                else
                    quantity = "Some";
            }
            else
                quantity = "None";

            GetValue += Get;
            Properties.Add(new PropertySpec("Quantity", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Quantity": e.Value = quantity; break;
            }
        }
    }

    [ExpandableObject]
    public class PolyhedronVertexParametersProperty : NestedProperty
    {
        public delegate bool ShowVertexParameters(out bool[] _validities, out float[] _params, out ulong _param_width);

        private string quantity;
        private ulong paramWidth;

        public PolyhedronVertexParametersProperty(
            ShowVertexParameters showVertexParameters)
            : base(null)
        {
            bool[] _validities;
            float[] _params;
            if (showVertexParameters(out _validities, out _params, out paramWidth))
            {
                if (Array.IndexOf(_validities, false) == -1)
                    quantity = "All";
                else
                    quantity = "Some";
            }
            else
                quantity = "None";

            GetValue += Get;
            Properties.Add(new PropertySpec("Quantity", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
            if (paramWidth > 0)
                Properties.Add(new PropertySpec("Width", typeof(ulong), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Quantity": e.Value = quantity; break;
                case "Width": e.Value = paramWidth; break;
            }
        }
    }

    [ExpandableObject]
    public class PolyhedronVisibilitiesProperty : NestedProperty
    {
        public delegate bool ShowVisibilities(out bool[] validities, out bool[] visibilities);

        private string quantity;

        public PolyhedronVisibilitiesProperty(
            ShowVisibilities showVisibilities)
            : base(null)
        {
            bool[] _validities;
            bool[] _visibilities;
            if (showVisibilities(out _validities, out _visibilities))
            {
                if (Array.IndexOf(_validities, false) == -1)
                    quantity = "All";
                else
                    quantity = "Some";
            }
            else
                quantity = "None";

            GetValue += Get;
            Properties.Add(new PropertySpec("Quantity", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Quantity": e.Value = quantity; break;
            }
        }
    }

    [ExpandableObject]
    public class PolyhedronFaceColorsProperty : NestedProperty
    {
        public delegate bool ShowFaceColors(out HPS.Material.Type[] types, out HPS.RGBColor[] colors, out float[] indices);

        private string quantity;

        public PolyhedronFaceColorsProperty(
            ShowFaceColors showFaceColors)
            : base(null)
        {
            HPS.Material.Type[] _types;
            HPS.RGBColor[] _rgb_colors;
            float[] _indices;
            if (showFaceColors(out _types, out _rgb_colors, out _indices))
            {
                if (Array.IndexOf(_types, HPS.Material.Type.None) == -1)
                    quantity = "All";
                else
                    quantity = "Some";
            }
            else
                quantity = "None";

            GetValue += Get;
            Properties.Add(new PropertySpec("Quantity", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Quantity": e.Value = quantity; break;
            }
        }
    }

    [ExpandableObject]
    public class PolyhedronMaterialMappingProperty<T> : NestedProperty
    {
        public delegate bool ShowMaterialMapping(out HPS.MaterialMappingKit materialMapping);
        public delegate T SetMaterialMapping(HPS.MaterialMappingKit materialMapping);
        public delegate T UnsetMaterialMapping();

        private SetMaterialMapping setMaterialMapping;
        private UnsetMaterialMapping unsetMaterialMapping;
        private bool set;
        private HPS.MaterialMappingKit kit;

        private ComplexMaterialProperty frontFaceMaterial;
        private ComplexMaterialProperty backFaceMaterial;
        private ComplexMaterialProperty edgeMaterial;
        private ComplexMaterialProperty vertexMaterial;

        public PolyhedronMaterialMappingProperty(
            ShowMaterialMapping showMaterialMapping,
            SetMaterialMapping setMaterialMapping,
            UnsetMaterialMapping unsetMaterialMapping)
            : base(null)
        {
            this.setMaterialMapping = setMaterialMapping;
            this.unsetMaterialMapping = unsetMaterialMapping;

            set = showMaterialMapping(out kit);

            frontFaceMaterial = new ComplexMaterialProperty(kit.ShowFrontFaceMaterial, kit.SetFrontFaceMaterial, kit.SetFrontFaceMaterialByIndex, kit.UnsetFrontFaceMaterial);
            backFaceMaterial = new ComplexMaterialProperty(kit.ShowBackFaceMaterial, kit.SetBackFaceMaterial, kit.SetBackFaceMaterialByIndex, kit.UnsetBackFaceMaterial);
            edgeMaterial = new ComplexMaterialProperty(kit.ShowEdgeMaterial, kit.SetEdgeMaterial, kit.SetEdgeMaterialByIndex, kit.UnsetEdgeMaterial);
            vertexMaterial = new ComplexMaterialProperty(kit.ShowVertexMaterial, kit.SetVertexMaterial, kit.SetVertexMaterialByIndex, kit.UnsetVertexMaterial);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("FrontFaceMaterial", typeof(ComplexMaterialProperty), null, expandable: true));
            Properties.Add(new PropertySpec("BackFaceMaterial", typeof(ComplexMaterialProperty), null, expandable: true));
            Properties.Add(new PropertySpec("EdgeMaterial", typeof(ComplexMaterialProperty), null, expandable: true));
            Properties.Add(new PropertySpec("VertexMaterial", typeof(ComplexMaterialProperty), null, expandable: true));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "FrontFaceMaterial": e.Value = frontFaceMaterial; break;
                case "BackFaceMaterial": e.Value = backFaceMaterial; break;
                case "EdgeMaterial": e.Value = edgeMaterial; break;
                case "VertexMaterial": e.Value = vertexMaterial; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                        UpdateKit();
                    }
                    break;
                case "FrontFaceMaterial":
                    {
                        // nothing to do
                    }
                    break;
                case "BackFaceMaterial":
                    {
                        // nothing to do
                    }
                    break;
                case "EdgeMaterial":
                    {
                        // nothing to do
                    }
                    break;
                case "VertexMaterial":
                    {
                        // nothing to do
                    }
                    break;
            }
        }

        private void UpdateKit()
        {
            if (set)
                setMaterialMapping(kit);
            else
                unsetMaterialMapping();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    [DisplayName("Shell")]
    public class ShellKitProperty : IRootProperty
    {
        private HPS.ShellKey key;
        private HPS.ShellKit kit;

        public ShellKitProperty(
            HPS.ShellKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Points = new GeometryPointCountProperty(kit.GetPointCount);
            Facelist = new ShellKitFacelistProperty(kit);
            VertexColors = new ShellKitVertexColorsProperty(kit);
            VertexNormals = new PolyhedronNormalsProperty(kit.ShowVertexNormals);
            VertexParameters = new PolyhedronVertexParametersProperty(kit.ShowVertexParameters);
            VertexVisibilities = new PolyhedronVisibilitiesProperty(kit.ShowVertexVisibilities);
            FaceColors = new PolyhedronFaceColorsProperty(kit.ShowFaceColors);
            FaceNormals = new PolyhedronNormalsProperty(kit.ShowFaceNormals);
            FaceVisibilities = new PolyhedronVisibilitiesProperty(kit.ShowFaceVisibilities);
            Priority = new PriorityProperty<HPS.ShellKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.ShellKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
            MaterialMapping = new PolyhedronMaterialMappingProperty<HPS.ShellKit>(kit.ShowMaterialMapping, kit.SetMaterialMapping, kit.UnsetMaterialMapping);
        }

        [PropertyOrder(0)]
        public GeometryPointCountProperty Points { get; set; }

        [PropertyOrder(1)]
        public ShellKitFacelistProperty Facelist { get; set; }

        [PropertyOrder(2)]
        public ShellKitVertexColorsProperty VertexColors { get; set; }

        [PropertyOrder(3)]
        public PolyhedronNormalsProperty VertexNormals { get; set; }

        [PropertyOrder(4)]
        public PolyhedronVertexParametersProperty VertexParameters { get; set; }

        [PropertyOrder(5)]
        public PolyhedronVisibilitiesProperty VertexVisibilities { get; set; }

        [PropertyOrder(6)]
        public PolyhedronFaceColorsProperty FaceColors { get; set; }

        [PropertyOrder(7)]
        public PolyhedronNormalsProperty FaceNormals { get; set; }

        [PropertyOrder(8)]
        public PolyhedronVisibilitiesProperty FaceVisibilities { get; set; }

        [PropertyOrder(9)]
        public PriorityProperty<HPS.ShellKit> Priority { get; set; }

        [PropertyOrder(10)]
        public UserDataProperty<HPS.ShellKit> UserData { get; set; }

        [PropertyOrder(11)]
        public PolyhedronMaterialMappingProperty<HPS.ShellKit> MaterialMapping { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region MeshKit
    [ExpandableObject]
    public class MeshKitPointsProperty : NestedProperty
    {
        private ulong pointCount;
        private ulong rows;
        private ulong columns;

        public MeshKitPointsProperty(
            HPS.MeshKit kit)
            : base(null)
        {
            pointCount = kit.GetPointCount();
            kit.ShowRows(out rows);
            kit.ShowColumns(out columns);

            GetValue += Get;
            Properties.Add(new PropertySpec("Count", typeof(ulong), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Rows", typeof(ulong), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Columns", typeof(ulong), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = pointCount; break;
                case "Rows": e.Value = rows; break;
                case "Columns": e.Value = columns; break;
            }
        }
    }

    [ExpandableObject]
    public class MeshKitVertexColorsProperty : NestedProperty
    {
        private string faceQuantity;
        private string edgeQuantity;
        private string vertexQuantity;

        public MeshKitVertexColorsProperty(
            HPS.MeshKit kit)
            : base(null)
        {
            faceQuantity = VertexUtilities.GetVertexColorQuantity<HPS.Mesh.Component>(kit.ShowVertexColors, HPS.Mesh.Component.Faces);
            edgeQuantity = VertexUtilities.GetVertexColorQuantity<HPS.Mesh.Component>(kit.ShowVertexColors, HPS.Mesh.Component.Edges);
            vertexQuantity = VertexUtilities.GetVertexColorQuantity<HPS.Mesh.Component>(kit.ShowVertexColors, HPS.Mesh.Component.Vertices);

            GetValue += Get;
            Properties.Add(new PropertySpec("Faces", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Edges", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Vertices", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Faces": e.Value = faceQuantity; break;
                case "Edges": e.Value = edgeQuantity; break;
                case "Vertices": e.Value = vertexQuantity; break;
            }
        }
    }

    [DisplayName("Mesh")]
    public class MeshKitProperty : IRootProperty
    {
        private HPS.MeshKey key;
        private HPS.MeshKit kit;

        public MeshKitProperty(
            HPS.MeshKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Points = new MeshKitPointsProperty(kit);
            VertexColors = new MeshKitVertexColorsProperty(kit);
            VertexNormals = new PolyhedronNormalsProperty(kit.ShowVertexNormals);
            VertexParameters = new PolyhedronVertexParametersProperty(kit.ShowVertexParameters);
            VertexVisibilities = new PolyhedronVisibilitiesProperty(kit.ShowVertexVisibilities);
            FaceColors = new PolyhedronFaceColorsProperty(kit.ShowFaceColors);
            FaceNormals = new PolyhedronNormalsProperty(kit.ShowFaceNormals);
            FaceVisibilities = new PolyhedronVisibilitiesProperty(kit.ShowFaceVisibilities);
            Priority = new PriorityProperty<HPS.MeshKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.MeshKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
            MaterialMapping = new PolyhedronMaterialMappingProperty<HPS.MeshKit>(kit.ShowMaterialMapping, kit.SetMaterialMapping, kit.UnsetMaterialMapping);
        }

        [PropertyOrder(0)]
        public MeshKitPointsProperty Points { get; set; }

        [PropertyOrder(1)]
        public MeshKitVertexColorsProperty VertexColors { get; set; }

        [PropertyOrder(2)]
        public PolyhedronNormalsProperty VertexNormals { get; set; }

        [PropertyOrder(3)]
        public PolyhedronVertexParametersProperty VertexParameters { get; set; }

        [PropertyOrder(4)]
        public PolyhedronVisibilitiesProperty VertexVisibilities { get; set; }

        [PropertyOrder(5)]
        public PolyhedronFaceColorsProperty FaceColors { get; set; }

        [PropertyOrder(6)]
        public PolyhedronNormalsProperty FaceNormals { get; set; }

        [PropertyOrder(7)]
        public PolyhedronVisibilitiesProperty FaceVisibilities { get; set; }

        [PropertyOrder(8)]
        public PriorityProperty<HPS.MeshKit> Priority { get; set; }

        [PropertyOrder(9)]
        public UserDataProperty<HPS.MeshKit> UserData { get; set; }

        [PropertyOrder(10)]
        public PolyhedronMaterialMappingProperty<HPS.MeshKit> MaterialMapping { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region CylinderKit
    [ExpandableObject]
    public class ImmutableArrayCountProperty<T> : NestedProperty
    {
        public delegate bool ShowArray(out T[] values);

        private string propertyName;
        private int valueCount;

        public ImmutableArrayCountProperty(
            ShowArray showArray,
            string propertyName = "Count")
            : base(null)
        {
            this.propertyName = propertyName;

            T[] values;
            showArray(out values);
            valueCount = values.Length;

            GetValue += Get;
            Properties.Add(new PropertySpec(this.propertyName, typeof(int), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            if (e.Property.Name == propertyName)
                e.Value = valueCount;
        }
    }

    [ExpandableObject]
    public class CylinderKitCapsProperty : NestedProperty
    {
        private HPS.CylinderKit kit;
        private HPS.Cylinder.Capping _caps;

        public CylinderKitCapsProperty(
            HPS.CylinderKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowCaps(out _caps);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Caps", typeof(HPS.Cylinder.Capping), null, expandable: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Caps": e.Value = _caps; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Caps":
                    {
                        _caps = (HPS.Cylinder.Capping)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetCaps(_caps);
        }
    }

    [ExpandableObject]
    public class CylinderKitVertexColorsProperty : NestedProperty
    {
        private string faceQuantity;
        private string edgeQuantity;

        public CylinderKitVertexColorsProperty(
            HPS.CylinderKit kit)
            : base(null)
        {
            faceQuantity = GetVertexColorQuantity(kit, HPS.Cylinder.Component.Faces);
            edgeQuantity = GetVertexColorQuantity(kit, HPS.Cylinder.Component.Edges);

            GetValue += Get;
            Properties.Add(new PropertySpec("Faces", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Edges", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private static string GetVertexColorQuantity(
            HPS.CylinderKit kit,
            HPS.Cylinder.Component component)
        {
            HPS.Material.Type[] types;
            HPS.RGBColor[] rgbColors;
            float[] indices;
            if (kit.ShowVertexColors(component, out types, out rgbColors, out indices))
            {
                if (Array.IndexOf(types, HPS.Material.Type.None) == -1)
                    return "All";
                else
                    return "Some";
            }
            else
                return "None";
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Faces": e.Value = faceQuantity; break;
                case "Edges": e.Value = edgeQuantity; break;
            }
        }
    }

    [DisplayName("Cylinder")]
    public class CylinderKitProperty : IRootProperty
    {
        private HPS.CylinderKey key;
        private HPS.CylinderKit kit;

        public CylinderKitProperty(
            HPS.CylinderKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Points = new GeometryPointCountProperty(kit.GetPointCount);
            Radii = new ImmutableArrayCountProperty<float>(kit.ShowRadii);
            Caps = new CylinderKitCapsProperty(kit);
            VertexColors = new CylinderKitVertexColorsProperty(kit);
            Priority = new PriorityProperty<HPS.CylinderKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.CylinderKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public GeometryPointCountProperty Points { get; set; }

        [PropertyOrder(1)]
        public ImmutableArrayCountProperty<float> Radii { get; set; }

        [PropertyOrder(2)]
        public CylinderKitCapsProperty Caps { get; set; }

        [PropertyOrder(3)]
        public CylinderKitVertexColorsProperty VertexColors { get; set; }

        [PropertyOrder(4)]
        public PriorityProperty<HPS.CylinderKit> Priority { get; set; }

        [PropertyOrder(5)]
        public UserDataProperty<HPS.CylinderKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region LineKit
    [DisplayName("Line")]
    public class LineKitProperty : IRootProperty
    {
        private HPS.LineKey key;
        private HPS.LineKit kit;

        public LineKitProperty(
            HPS.LineKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Points = new GeometryPointCountProperty(kit.GetPointCount);
            Priority = new PriorityProperty<HPS.LineKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.LineKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public GeometryPointCountProperty Points { get; set; }

        [PropertyOrder(1)]
        public PriorityProperty<HPS.LineKit> Priority { get; set; }

        [PropertyOrder(2)]
        public UserDataProperty<HPS.LineKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region NURBSCurveKit
    [ExpandableObject]
    public class NURBSCurveKitDegreeProperty : NestedProperty
    {
        private ulong degree;

        public NURBSCurveKitDegreeProperty(
            HPS.NURBSCurveKit kit)
            : base(null)
        {
            kit.ShowDegree(out degree);

            GetValue += Get;
            Properties.Add(new PropertySpec("Degree", typeof(ulong), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Degree": e.Value = degree; break;
            }
        }
    }

    [ExpandableObject]
    public class NURBSCurveKitParametersProperty : NestedProperty
    {
        private HPS.NURBSCurveKit kit;
        private float _start;
        private float _end;

        public NURBSCurveKitParametersProperty(
            HPS.NURBSCurveKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowParameters(out _start, out _end);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Start", typeof(float), null, expandable: false));
            Properties.Add(new PropertySpec("End", typeof(float), null, expandable: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Start": e.Value = _start; break;
                case "End": e.Value = _end; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Start":
                    {
                        _start = (float)e.Value;
                    }
                    break;
                case "End":
                    {
                        _end = (float)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetParameters(_start, _end);
        }
    }

    [DisplayName("NURBSCurve")]
    public class NURBSCurveKitProperty : IRootProperty
    {
        private HPS.NURBSCurveKey key;
        private HPS.NURBSCurveKit kit;

        public NURBSCurveKitProperty(
            HPS.NURBSCurveKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Degree = new NURBSCurveKitDegreeProperty(kit);
            Points = new GeometryPointCountProperty(kit.GetPointCount);
            Weights = new ImmutableArrayCountProperty<float>(kit.ShowWeights);
            Knots = new ImmutableArrayCountProperty<float>(kit.ShowKnots);
            Parameters = new NURBSCurveKitParametersProperty(kit);
            Priority = new PriorityProperty<HPS.NURBSCurveKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.NURBSCurveKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public NURBSCurveKitDegreeProperty Degree { get; set; }

        [PropertyOrder(1)]
        public GeometryPointCountProperty Points { get; set; }

        [PropertyOrder(2)]
        public ImmutableArrayCountProperty<float> Weights { get; set; }

        [PropertyOrder(3)]
        public ImmutableArrayCountProperty<float> Knots { get; set; }

        [PropertyOrder(4)]
        public NURBSCurveKitParametersProperty Parameters { get; set; }

        [PropertyOrder(5)]
        public PriorityProperty<HPS.NURBSCurveKit> Priority { get; set; }

        [PropertyOrder(6)]
        public UserDataProperty<HPS.NURBSCurveKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region NURBSSurfaceKit
    [ExpandableObject]
    public class NURBSSurfaceKitDegreeCountProperty : NestedProperty
    {
        public delegate bool ShowDegree(out ulong degree);
        public delegate bool ShowCount(out ulong count);

        private ulong degree;
        private ulong count;

        public NURBSSurfaceKitDegreeCountProperty(
            ShowDegree showDegree,
            ShowCount showCount)
            : base(null)
        {
            showDegree(out degree);
            showCount(out count);

            GetValue += Get;
            Properties.Add(new PropertySpec("Degree", typeof(ulong), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Count", typeof(ulong), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Degree": e.Value = degree; break;
                case "Count": e.Value = count; break;
            }
        }
    }

    [ExpandableObject]
    public class NURBSSurfaceKitKnotsProperty : NestedProperty
    {
        private int uKnotCount;
        private int vKnotCount;

        public NURBSSurfaceKitKnotsProperty(
            HPS.NURBSSurfaceKit kit)
            : base(null)
        {
            float[] knots;
            kit.ShowUKnots(out knots);
            uKnotCount = knots.Length;
            kit.ShowVKnots(out knots);
            vKnotCount = knots.Length;

            GetValue += Get;
            Properties.Add(new PropertySpec("U Count", typeof(int), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("V Count", typeof(int), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "U Count": e.Value = uKnotCount; break;
                case "V Count": e.Value = vKnotCount; break;
            }
        }
    }

    [DisplayName("NURBSSurface")]
    public class NURBSSurfaceKitProperty : IRootProperty
    {
        private HPS.NURBSSurfaceKey key;
        private HPS.NURBSSurfaceKit kit;

        public NURBSSurfaceKitProperty(
            HPS.NURBSSurfaceKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            U = new NURBSSurfaceKitDegreeCountProperty(kit.ShowUDegree, kit.ShowUCount);
            V = new NURBSSurfaceKitDegreeCountProperty(kit.ShowVDegree, kit.ShowVCount);
            Points = new GeometryPointCountProperty(kit.GetPointCount);
            Weights = new ImmutableArrayCountProperty<float>(kit.ShowWeights);
            Knots = new NURBSSurfaceKitKnotsProperty(kit);
            Trims = new ImmutableArrayCountProperty<HPS.TrimKit>(kit.ShowTrims);
            Priority = new PriorityProperty<HPS.NURBSSurfaceKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.NURBSSurfaceKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public NURBSSurfaceKitDegreeCountProperty U { get; set; }

        [PropertyOrder(1)]
        public NURBSSurfaceKitDegreeCountProperty V { get; set; }

        [PropertyOrder(2)]
        public GeometryPointCountProperty Points { get; set; }

        [PropertyOrder(3)]
        public ImmutableArrayCountProperty<float> Weights { get; set; }

        [PropertyOrder(4)]
        public NURBSSurfaceKitKnotsProperty Knots { get; set; }

        [PropertyOrder(5)]
        public ImmutableArrayCountProperty<HPS.TrimKit> Trims { get; set; }

        [PropertyOrder(6)]
        public PriorityProperty<HPS.NURBSSurfaceKit> Priority { get; set; }

        [PropertyOrder(7)]
        public UserDataProperty<HPS.NURBSSurfaceKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region PolygonKit
    [DisplayName("Polygon")]
    public class PolygonKitProperty : IRootProperty
    {
        private HPS.PolygonKey key;
        private HPS.PolygonKit kit;

        public PolygonKitProperty(
            HPS.PolygonKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Points = new GeometryPointCountProperty(kit.GetPointCount);
            Priority = new PriorityProperty<HPS.PolygonKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.PolygonKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public GeometryPointCountProperty Points { get; set; }

        [PropertyOrder(1)]
        public PriorityProperty<HPS.PolygonKit> Priority { get; set; }

        [PropertyOrder(2)]
        public UserDataProperty<HPS.PolygonKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region ImageKit
    [ExpandableObject]
    public class ImageKitSizeProperty : NestedProperty
    {
        private uint _width;
        private uint _height;

        public ImageKitSizeProperty(
            HPS.ImageKit kit)
            : base(null)
        {
            kit.ShowSize(out _width, out _height);

            GetValue += Get;
            Properties.Add(new PropertySpec("Width", typeof(uint), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Height", typeof(uint), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Width": e.Value = _width; break;
                case "Height": e.Value = _height; break;
            }
        }
    }

    [ExpandableObject]
    public class ImageKitFormatProperty : NestedProperty
    {
        private HPS.Image.Format _format;

        public ImageKitFormatProperty(
            HPS.ImageKit kit)
            : base(null)
        {
            kit.ShowFormat(out _format);

            GetValue += Get;
            Properties.Add(new PropertySpec("Format", typeof(HPS.Image.Format), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Format": e.Value = _format; break;
            }
        }
    }

    [ExpandableObject]
    public class ImageKitDownSamplingProperty : NestedProperty
    {
        private HPS.ImageKit kit;
        private bool _state;

        public ImageKitDownSamplingProperty(
            HPS.ImageKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowDownSampling(out _state);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                    {
                        _state = (bool)e.Value;
                        kit.SetDownSampling(_state);
                    }
                    break;
            }
        }
    }

    [ExpandableObject]
    public class ImageKitCompressionQualityProperty : NestedProperty
    {
        private HPS.ImageKit kit;
        private float _quality;

        public ImageKitCompressionQualityProperty(
            HPS.ImageKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowCompressionQuality(out _quality);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Quality", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Quality": e.Value = _quality; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Quality":
                    {
                        _quality = (float)e.Value;
                        kit.SetCompressionQuality(_quality);
                    }
                    break;
            }
        }
    }

    [DisplayName("Image")]
    public class ImageDefinitionProperty : IRootProperty
    {
        private HPS.ImageDefinition definition;
        private HPS.ImageKit kit;

        public ImageDefinitionProperty(
            HPS.ImageDefinition definition)
        {
            this.definition = definition;
            this.definition.Show(out kit);

            Name = this.definition.Name();
            Size = new ImageKitSizeProperty(kit);
            Data = new ImmutableArrayCountProperty<byte>(kit.ShowData, "Byte Count");
            Format = new ImageKitFormatProperty(kit);
            DownSampling = new ImageKitDownSamplingProperty(kit);
            CompressionQuality = new ImageKitCompressionQualityProperty(kit);
        }

        [PropertyOrder(0)]
        public string Name { get; private set; }

        [PropertyOrder(1)]
        public ImageKitSizeProperty Size { get; set; }

        [PropertyOrder(2)]
        public ImmutableArrayCountProperty<byte> Data { get; set; }

        [PropertyOrder(3)]
        public ImageKitFormatProperty Format { get; set; }

        [PropertyOrder(4)]
        public ImageKitDownSamplingProperty DownSampling { get; set; }

        [PropertyOrder(5)]
        public ImageKitCompressionQualityProperty CompressionQuality { get; set; }

        public void Apply()
        {
            definition.Set(kit);
        }
    }
    #endregion

    #region LegacyShaderKit
    [ExpandableObject]
    public class LegacyShaderKitSourceProperty : NestedProperty
    {
        private string _source;

        public LegacyShaderKitSourceProperty(
            HPS.LegacyShaderKit kit)
            : base(null)
        {
            kit.ShowSource(out _source);

            GetValue += Get;
            Properties.Add(new PropertySpec("Character Count", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Source": e.Value = _source.Length; break;
            }
        }
    }

    [ExpandableObject]
    public class LegacyShaderKitMultitextureProperty : NestedProperty
    {
        private HPS.LegacyShaderKit kit;
        private bool _state;

        public LegacyShaderKitMultitextureProperty(
            HPS.LegacyShaderKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowMultitexture(out _state);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                    {
                        _state = (bool)e.Value;
                        kit.SetMultitexture(_state);
                    }
                    break;
            }
        }
    }

    [ExpandableObject]
    public class LegacyShaderKitParameterizationSourceProperty : NestedProperty
    {
        private HPS.LegacyShaderKit kit;
        private HPS.LegacyShader.Parameterization _source;

        public LegacyShaderKitParameterizationSourceProperty(
            HPS.LegacyShaderKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowParameterizationSource(out _source);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Source", typeof(HPS.LegacyShader.Parameterization), null));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Source": e.Value = _source; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Source":
                    {
                        _source = (HPS.LegacyShader.Parameterization)e.Value;
                        kit.SetParameterizationSource(_source);
                    }
                    break;
            }
        }
    }

    public class LegacyShaderKitTransformMatrixProperty : KitMatrixProperty<HPS.LegacyShaderKit>
    {
        public LegacyShaderKitTransformMatrixProperty(
            HPS.LegacyShaderKit kit)
            : base(kit.ShowTransformMatrix, kit.SetTransformMatrix, kit.UnsetTransformMatrix)
        { }
    }

    [DisplayName("LegacyShader")]
    public class LegacyShaderDefinitionProperty : IRootProperty
    {
        private HPS.LegacyShaderDefinition definition;
        private HPS.LegacyShaderKit kit;

        public LegacyShaderDefinitionProperty(
            HPS.LegacyShaderDefinition definition)
        {
            this.definition = definition;
            this.definition.Show(out kit);

            Name = this.definition.Name();
            Source = new LegacyShaderKitSourceProperty(kit);
            Multitexture = new LegacyShaderKitMultitextureProperty(kit);
            ParameterizationSource = new LegacyShaderKitParameterizationSourceProperty(kit);
            TransformMatrix = new LegacyShaderKitTransformMatrixProperty(kit);
        }

        [PropertyOrder(0)]
        public string Name { get; private set; }

        [PropertyOrder(0)]
        public LegacyShaderKitSourceProperty Source { get; set; }

        [PropertyOrder(1)]
        public LegacyShaderKitMultitextureProperty Multitexture { get; set; }

        [PropertyOrder(2)]
        public LegacyShaderKitParameterizationSourceProperty ParameterizationSource { get; set; }

        [PropertyOrder(3)]
        public LegacyShaderKitTransformMatrixProperty TransformMatrix { get; set; }

        public void Apply()
        {
            definition.Set(kit);
        }
    }
    #endregion

    #region TextureOptionsKit
    [ExpandableObject]
    public class TextureOptionsKitDecalProperty : NestedProperty
    {
        private HPS.TextureOptionsKit kit;
        private bool _state;

        public TextureOptionsKitDecalProperty(
            HPS.TextureOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowDecal(out _state);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetDecal(_state);
        }
    }

    [ExpandableObject]
    public class TextureOptionsKitDownSamplingProperty : NestedProperty
    {
        private HPS.TextureOptionsKit kit;
        private bool _state;

        public TextureOptionsKitDownSamplingProperty(
            HPS.TextureOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowDownSampling(out _state);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetDownSampling(_state);
        }
    }

    [ExpandableObject]
    public class TextureOptionsKitModulationProperty : NestedProperty
    {
        private HPS.TextureOptionsKit kit;
        private bool _state;

        public TextureOptionsKitModulationProperty(
            HPS.TextureOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowModulation(out _state);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetModulation(_state);
        }
    }

    [ExpandableObject]
    public class TextureOptionsKitParameterOffsetProperty : NestedProperty
    {
        private HPS.TextureOptionsKit kit;
        private ulong _offset;

        public TextureOptionsKitParameterOffsetProperty(
            HPS.TextureOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowParameterOffset(out _offset);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Offset", typeof(ulong), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Offset": e.Value = _offset; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Offset":
                    {
                        _offset = (ulong)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetParameterOffset(_offset);
        }
    }

    [ExpandableObject]
    public class TextureOptionsKitParameterizationSourceProperty : NestedProperty
    {
        private HPS.TextureOptionsKit kit;
        private HPS.Material.Texture.Parameterization _source;

        public TextureOptionsKitParameterizationSourceProperty(
            HPS.TextureOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowParameterizationSource(out _source);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Source", typeof(HPS.Material.Texture.Parameterization), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Source": e.Value = _source; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Source":
                    {
                        _source = (HPS.Material.Texture.Parameterization)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetParameterizationSource(_source);
        }
    }

    [ExpandableObject]
    public class TextureOptionsKitTilingProperty : NestedProperty
    {
        private HPS.TextureOptionsKit kit;
        private HPS.Material.Texture.Tiling _tiling;

        public TextureOptionsKitTilingProperty(
            HPS.TextureOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowTiling(out _tiling);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Tiling", typeof(HPS.Material.Texture.Tiling), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Tiling": e.Value = _tiling; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Tiling":
                    {
                        _tiling = (HPS.Material.Texture.Tiling)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetTiling(_tiling);
        }
    }

    [ExpandableObject]
    public class TextureOptionsKitInterpolationFilterProperty : NestedProperty
    {
        private HPS.TextureOptionsKit kit;
        private HPS.Material.Texture.Interpolation _filter;

        public TextureOptionsKitInterpolationFilterProperty(
            HPS.TextureOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowInterpolationFilter(out _filter);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Filter", typeof(HPS.Material.Texture.Interpolation), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Filter": e.Value = _filter; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Filter":
                    {
                        _filter = (HPS.Material.Texture.Interpolation)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetInterpolationFilter(_filter);
        }
    }

    [ExpandableObject]
    public class TextureOptionsKitDecimationFilterProperty : NestedProperty
    {
        private HPS.TextureOptionsKit kit;
        private HPS.Material.Texture.Decimation _filter;

        public TextureOptionsKitDecimationFilterProperty(
            HPS.TextureOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowDecimationFilter(out _filter);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Filter", typeof(HPS.Material.Texture.Decimation), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Filter": e.Value = _filter; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Filter":
                    {
                        _filter = (HPS.Material.Texture.Decimation)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetDecimationFilter(_filter);
        }
    }

    public class TextureOptionsKitTransformMatrixProperty : KitMatrixProperty<HPS.TextureOptionsKit>
    {
        public TextureOptionsKitTransformMatrixProperty(
            HPS.TextureOptionsKit kit)
            : base(kit.ShowTransformMatrix, kit.SetTransformMatrix, kit.UnsetTransformMatrix)
        { }
    }

    public class TextureOrCubemapProperty : IRootProperty
    {
        public delegate string GetName();
        public delegate void ShowOptions(out HPS.TextureOptionsKit kit);
        public delegate void SetOptions(HPS.TextureOptionsKit kit);

        private SetOptions setOptions;
        private HPS.TextureOptionsKit kit;

        public TextureOrCubemapProperty(
            GetName getName,
            ShowOptions showOptions,
            SetOptions setOptions)
        {
            this.setOptions = setOptions;
            showOptions(out kit);

            Name = getName();
            Decal = new TextureOptionsKitDecalProperty(kit);
            DownSampling = new TextureOptionsKitDownSamplingProperty(kit);
            Modulation = new TextureOptionsKitModulationProperty(kit);
            ParameterOffset = new TextureOptionsKitParameterOffsetProperty(kit);
            ParameterizationSource = new TextureOptionsKitParameterizationSourceProperty(kit);
            Tiling = new TextureOptionsKitTilingProperty(kit);
            InterpolationFilter = new TextureOptionsKitInterpolationFilterProperty(kit);
            DecimationFilter = new TextureOptionsKitDecimationFilterProperty(kit);
            TransformMatrix = new TextureOptionsKitTransformMatrixProperty(kit);
        }

        [PropertyOrder(0)]
        public string Name { get; private set; }

        [PropertyOrder(1)]
        public TextureOptionsKitDecalProperty Decal { get; set; }

        [PropertyOrder(2)]
        public TextureOptionsKitDownSamplingProperty DownSampling { get; set; }

        [PropertyOrder(3)]
        public TextureOptionsKitModulationProperty Modulation { get; set; }

        [PropertyOrder(4)]
        public TextureOptionsKitParameterOffsetProperty ParameterOffset { get; set; }

        [PropertyOrder(5)]
        public TextureOptionsKitParameterizationSourceProperty ParameterizationSource { get; set; }

        [PropertyOrder(6)]
        public TextureOptionsKitTilingProperty Tiling { get; set; }

        [PropertyOrder(7)]
        public TextureOptionsKitInterpolationFilterProperty InterpolationFilter { get; set; }

        [PropertyOrder(8)]
        public TextureOptionsKitDecimationFilterProperty DecimationFilter { get; set; }

        [PropertyOrder(9)]
        public TextureOptionsKitTransformMatrixProperty TransformMatrix { get; set; }

        public void Apply()
        {
            setOptions(kit);
        }
    }
    #endregion

    #region TextureDefinition
    [DisplayName("Texture Definition")]
    public class TextureDefinitionProperty : TextureOrCubemapProperty
    {
        public TextureDefinitionProperty(
            HPS.TextureDefinition definition)
            : base(definition.Name, definition.ShowOptions, definition.SetOptions)
        { }
    }
    #endregion

    #region CubeMapDefinition
    [DisplayName("CubeMap Definition")]
    public class CubeMapDefinitionProperty : TextureOrCubemapProperty
    {
        public CubeMapDefinitionProperty(
            HPS.CubeMapDefinition definition)
            : base(definition.Name, definition.ShowOptions, definition.SetOptions)
        { }
    }
    #endregion

    #region LinePatternDefinition
    [ExpandableObject]
    public class LinePatternKitJoinProperty : NestedProperty
    {
        private HPS.LinePatternKit kit;
        private bool set;
        private HPS.LinePattern.Join _type;

        public LinePatternKitJoinProperty(
            HPS.LinePatternKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowJoin(out _type);
            if (!set)
            {
                _type = HPS.LinePattern.Join.Mitre;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Type", typeof(HPS.LinePattern.Join), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Type": e.Value = _type; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Type":
                    {
                        _type = (HPS.LinePattern.Join)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetJoin(_type);
            else
                kit.UnsetJoin();
        }
    }

    [DisplayName("LinePattern")]
    public class LinePatternDefinitionProperty : IRootProperty
    {
        private HPS.LinePatternDefinition definition;
        private HPS.LinePatternKit kit;

        public LinePatternDefinitionProperty(
            HPS.LinePatternDefinition definition)
        {
            this.definition = definition;
            this.definition.Show(out kit);

            Name = this.definition.Name();
            Parallels = new ImmutableArrayCountProperty<HPS.LinePatternParallelKit>(kit.ShowParallels);
            Join = new LinePatternKitJoinProperty(kit);
        }

        [PropertyOrder(0)]
        public string Name { get; private set; }

        [PropertyOrder(1)]
        public ImmutableArrayCountProperty<HPS.LinePatternParallelKit> Parallels { get; set; }

        [PropertyOrder(2)]
        public LinePatternKitJoinProperty Join { get; set; }

        public void Apply()
        {
            definition.Set(kit);
        }
    }
    #endregion

    #region GlyphDefinition
    [ExpandableObject]
    public class GlyphKitRadiusProperty : NestedProperty
    {
        private HPS.GlyphKit kit;
        private sbyte _radius;

        public GlyphKitRadiusProperty(
            HPS.GlyphKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowRadius(out _radius);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Radius", typeof(sbyte), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Radius": e.Value = _radius; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Radius":
                    {
                        _radius = (sbyte)e.Value;
                        kit.SetRadius(_radius);
                    }
                    break;
            }
        }
    }

    [ExpandableObject]
    public class GlyphKitOffsetProperty : NestedProperty
    {
        private HPS.GlyphKit kit;
        private HPS.GlyphPoint point;

        private GlyphPointProperty pointProperty;

        public GlyphKitOffsetProperty(
            HPS.GlyphKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowOffset(out point);

            GetValue += Get;
            pointProperty = new GlyphPointProperty(point, UpdatePoint);
            Properties.Add(new PropertySpec("Point", typeof(GlyphPointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Point": e.Value = pointProperty; break;
            }
        }

        private void UpdatePoint(
            HPS.GlyphPoint point)
        {
            this.point = point;
            kit.SetOffset(this.point);
        }
    }

    [DisplayName("Glyph")]
    public class GlyphDefinitionProperty : IRootProperty
    {
        private HPS.GlyphDefinition definition;
        private HPS.GlyphKit kit;

        public GlyphDefinitionProperty(
            HPS.GlyphDefinition definition)
        {
            this.definition = definition;
            this.definition.Show(out kit);

            Name = this.definition.Name();
            Radius = new GlyphKitRadiusProperty(kit);
            Offset = new GlyphKitOffsetProperty(kit);
            Elements = new ImmutableArrayCountProperty<HPS.GlyphElement>(kit.ShowElements);
        }

        [PropertyOrder(0)]
        public string Name { get; private set; }

        [PropertyOrder(1)]
        public GlyphKitRadiusProperty Radius { get; set; }

        [PropertyOrder(2)]
        public GlyphKitOffsetProperty Offset { get; set; }

        [PropertyOrder(3)]
        public ImmutableArrayCountProperty<HPS.GlyphElement> Elements { get; set; }

        public void Apply()
        {
            definition.Set(kit);
        }
    }
    #endregion

    #region ShapeDefinition
    [DisplayName("Shape Definition")]
    public class ShapeDefinitionProperty : IRootProperty
    {
        private HPS.ShapeDefinition definition;
        private HPS.ShapeKit kit;

        public ShapeDefinitionProperty(
            HPS.ShapeDefinition definition)
        {
            this.definition = definition;
            this.definition.Show(out kit);

            Name = this.definition.Name();
            Elements = new ImmutableArrayCountProperty<HPS.ShapeElement>(kit.ShowElements);
        }

        [PropertyOrder(0)]
        public string Name { get; private set; }

        [PropertyOrder(1)]
        public ImmutableArrayCountProperty<HPS.ShapeElement> Elements { get; set; }

        public void Apply()
        {
            // nothing to do
        }
    }
    #endregion

    #region MaterialPaletteDefinition
    public class MaterialPaletteDefinitionMaterialsArrayProperty : ArrayProperty
    {
        private int count;
        private HPS.MaterialKit[] materials;

        private ArrayList materialProperties;

        public MaterialPaletteDefinitionMaterialsArrayProperty(
            HPS.MaterialKit[] materials)
            : base(null, "Material")
        {
            this.materials = materials;

            count = this.materials.Length;
            materialProperties = new ArrayList();

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            int oldCount = materials.Length;
            Array.Resize(ref materials, newCount);
            for (int i = oldCount; i < newCount; ++i)
                materials[i] = new HPS.MaterialKit();
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                materialProperties.Add(new MaterialKitProperty(this, materials[i]));
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(MaterialKitProperty), null, expandable: true));
            }
        }

        protected override void DeleteProperties(
            int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; --i)
            {
                Properties.RemoveAt(i + 1);
                materialProperties.RemoveAt(i);
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = materialProperties[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
            }
        }

        public HPS.MaterialKit[] Materials
        {
            get { return materials; }
        }
    }

    [DisplayName("Material Palette Definition")]
    public class MaterialPaletteDefinitionProperty : IRootProperty
    {
        private HPS.MaterialPaletteDefinition definition;

        public MaterialPaletteDefinitionProperty(
            HPS.MaterialPaletteDefinition definition)
        {
            this.definition = definition;

            HPS.MaterialKit[] materials;
            this.definition.Show(out materials);

            Name = this.definition.Name();
            Materials = new MaterialPaletteDefinitionMaterialsArrayProperty(materials);
        }

        [PropertyOrder(0)]
        public string Name { get; private set; }

        [PropertyOrder(1)]
        public MaterialPaletteDefinitionMaterialsArrayProperty Materials { get; set; }

        public void Apply()
        {
            definition.Set(Materials.Materials);
        }
    }
    #endregion

    #region SubwindowKit
        [ExpandableObject]
    public class SubwindowKitSubwindowProperty : NestedProperty
    {
        private HPS.SubwindowKit kit;
        private bool set;
        private HPS.Rectangle _subwindow_position;
        private RectangleProperty _subwindow_positionProperty;
        private HPS.IntRectangle _subwindow_offsets;
        private IntRectangleProperty _subwindow_offsetsProperty;
        private HPS.Subwindow.Type _subwindow_type;

        public SubwindowKitSubwindowProperty(
            HPS.SubwindowKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSubwindow(out _subwindow_position, out _subwindow_offsets, out _subwindow_type);
            if (!set)
            {
                _subwindow_position = new HPS.Rectangle(-1.0f, 1.0f, -1.0f, 1.0f);
                _subwindow_offsets = HPS.IntRectangle.Zero();
                _subwindow_type = HPS.Subwindow.Type.Standard;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            _subwindow_positionProperty = new RectangleProperty(_subwindow_position, Update_subwindow_position);
            Properties.Add(new PropertySpec("Subwindow Position", typeof(RectangleProperty), null, expandable: true, triggersRefresh: false));
            _subwindow_offsetsProperty = new IntRectangleProperty(_subwindow_offsets, Update_subwindow_offsets);
            Properties.Add(new PropertySpec("Subwindow Offsets", typeof(IntRectangleProperty), null, expandable: true, triggersRefresh: false));
            Properties.Add(new PropertySpec("Subwindow Type", typeof(HPS.Subwindow.Type), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Subwindow Position": e.Value = _subwindow_positionProperty; break;
                case "Subwindow Offsets": e.Value = _subwindow_offsetsProperty; break;
                case "Subwindow Type": e.Value = _subwindow_type; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Subwindow Position":
                    {
                        // nothing to do
                    }
                    break;
                case "Subwindow Offsets":
                    {
                        // nothing to do
                    }
                    break;
                case "Subwindow Type":
                    {
                        _subwindow_type = (HPS.Subwindow.Type)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSubwindow(_subwindow_position, _subwindow_offsets, _subwindow_type);
            else
                kit.UnsetSubwindow();
        }

        private void Update_subwindow_position(
            HPS.Rectangle _subwindow_position)
        {
            this._subwindow_position = _subwindow_position;
            UpdateKit();
        }

        private void Update_subwindow_offsets(
            HPS.IntRectangle _subwindow_offsets)
        {
            this._subwindow_offsets = _subwindow_offsets;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class SubwindowKitBackgroundProperty : NestedProperty
    {
        private HPS.SubwindowKit kit;
        private bool set;
        private HPS.Subwindow.Background _bg_type;
        private string _definition_name;

        public SubwindowKitBackgroundProperty(
            HPS.SubwindowKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBackground(out _bg_type, out _definition_name);
            if (!set)
            {
                _bg_type = HPS.Subwindow.Background.SolidColor;
                _definition_name = "definition_name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Type", typeof(HPS.Subwindow.Background), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Definition Name", typeof(string), null, expandable: false, triggersRefresh: false));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Type": e.Value = _bg_type; break;
                case "Definition Name": e.Value = _definition_name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Type":
                    {
                        _bg_type = (HPS.Subwindow.Background)e.Value;
                        EnableValidProperties();
                    }
                    break;
                case "Definition Name":
                    {
                        _definition_name = (string)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Set = 0,
            Type = 1,
            Name = 2
        }

        private void EnableValidProperties()
        {
            if (_bg_type == HPS.Subwindow.Background.Image || _bg_type == HPS.Subwindow.Background.Cubemap)
                Properties[(int)PropertyTypeIndex.Name].Enabled = true;
            else
                Properties[(int)PropertyTypeIndex.Name].Enabled = false;
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBackground(_bg_type, _definition_name);
            else
                kit.UnsetBackground();
        }
    }

    [ExpandableObject]
    public class SubwindowKitBorderProperty : NestedProperty
    {
        private HPS.SubwindowKit kit;
        private bool set;
        private HPS.Subwindow.Border _border_type;

        public SubwindowKitBorderProperty(
            HPS.SubwindowKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBorder(out _border_type);
            if (!set)
            {
                _border_type = HPS.Subwindow.Border.None;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Border Type", typeof(HPS.Subwindow.Border), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Border Type": e.Value = _border_type; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Border Type":
                    {
                        _border_type = (HPS.Subwindow.Border)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBorder(_border_type);
            else
                kit.UnsetBorder();
        }
    }

    [ExpandableObject]
    public class SubwindowKitRenderingAlgorithmProperty : NestedProperty
    {
        private HPS.SubwindowKit kit;
        private bool set;
        private HPS.Subwindow.RenderingAlgorithm _hsra;

        public SubwindowKitRenderingAlgorithmProperty(
            HPS.SubwindowKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowRenderingAlgorithm(out _hsra);
            if (!set)
            {
                _hsra = HPS.Subwindow.RenderingAlgorithm.ZBuffer;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("HSRA", typeof(HPS.Subwindow.RenderingAlgorithm), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "HSRA": e.Value = _hsra; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "HSRA":
                    {
                        _hsra = (HPS.Subwindow.RenderingAlgorithm)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetRenderingAlgorithm(_hsra);
            else
                kit.UnsetRenderingAlgorithm();
        }
    }

    [ExpandableObject]
    public class SubwindowKitModelCompareModeProperty : NestedProperty
    {
        private HPS.SubwindowKit kit;
        private bool set;
        private bool _state;
        private HPS.SegmentKey _source1;
        private HPS.SegmentKey _source2;
        private string _source1Name;
        private string _source2Name;

        public SubwindowKitModelCompareModeProperty(
            HPS.SubwindowKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowModelCompareMode(out _state, out _source1, out _source2);
            if (set && _state)
            {
                _source1Name = _source1.Name();
                _source2Name = _source2.Name();
            }
            else
            {
                _state = false;
                _source1 = new HPS.SegmentKey();
                _source1Name = "No Segment";
                _source2 = new HPS.SegmentKey();
                _source2Name = "No Segment";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Source1", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Source2", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Source1": e.Value = _source1Name; break;
                case "Source2": e.Value = _source2Name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                        PropertyUtilities.EnableValidProperties(Properties, _state);
                    }
                    break;
                case "Source1":
                    {
                        // should be read-only
                        Debug.Assert(false);
                    }
                    break;
                case "Source2":
                    {
                        // should be read-only
                        Debug.Assert(false);
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetModelCompareMode(_state, _source1, _source2);
            else
                kit.UnsetModelCompareMode();
        }
    }

    [DisplayName("Subwindow")]
    public class SubwindowKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.SubwindowKit kit;

        public SubwindowKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowSubwindow(out kit);

            Subwindow = new SubwindowKitSubwindowProperty(kit);
            Background = new SubwindowKitBackgroundProperty(kit);
            Border = new SubwindowKitBorderProperty(kit);
            RenderingAlgorithm = new SubwindowKitRenderingAlgorithmProperty(kit);
            ModelCompareMode = new SubwindowKitModelCompareModeProperty(kit);
        }

        [PropertyOrder(0)]
        public SubwindowKitSubwindowProperty Subwindow { get; set; }

        [PropertyOrder(1)]
        public SubwindowKitBackgroundProperty Background { get; set; }

        [PropertyOrder(2)]
        public SubwindowKitBorderProperty Border { get; set; }

        [PropertyOrder(3)]
        public SubwindowKitRenderingAlgorithmProperty RenderingAlgorithm { get; set; }

        [PropertyOrder(4)]
        public SubwindowKitModelCompareModeProperty ModelCompareMode { get; set; }

        public void Apply()
        {
            key.UnsetSubwindow();
            key.SetSubwindow(kit);
        }
    }
    #endregion

    #region LinePatternOptionsKit
    [ExpandableObject]
    public class LinePatternOptionsKitCapOrJoinProperty<T> : NestedProperty
    {
        public delegate bool ShowCapOrJoin(out HPS.LinePattern.Modifier modifier, out string glyph, out T capOrJoin);
        public delegate HPS.LinePatternOptionsKit SetCapOrJoinGlyph(string glyph);
        public delegate HPS.LinePatternOptionsKit SetCapOrJoinEnum(T capOrJoin);
        public delegate HPS.LinePatternOptionsKit UnsetCapOrJoin();

        private SetCapOrJoinGlyph setCapOrJoinGlyph;
        private SetCapOrJoinEnum setCapOrJoinEnum;
        private UnsetCapOrJoin unsetCapOrJoin;
        private bool set;
        private HPS.LinePattern.Modifier _modifier;
        private string _glyph;
        private T _type;

        public LinePatternOptionsKitCapOrJoinProperty(
            NestedProperty owner,
            ShowCapOrJoin showCapOrJoin,
            SetCapOrJoinGlyph setCapOrJoinGlyph,
            SetCapOrJoinEnum setCapOrJoinEnum,
            UnsetCapOrJoin unsetCapOrJoin,
            T defaultEnumValue)
            : base(owner)
        {
            this.setCapOrJoinGlyph = setCapOrJoinGlyph;
            this.setCapOrJoinEnum = setCapOrJoinEnum;
            this.unsetCapOrJoin = unsetCapOrJoin;
            set = showCapOrJoin(out _modifier, out _glyph, out _type);
            if (!set)
            {
                _modifier = HPS.LinePattern.Modifier.Enumerated;
                _glyph = "glyph";
                _type = defaultEnumValue;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Modifier", typeof(HPS.LinePattern.Modifier), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Glyph", typeof(string), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Type", typeof(T), null, expandable: false, triggersRefresh: false));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Modifier": e.Value = _modifier; break;
                case "Glyph": e.Value = _glyph; break;
                case "Type": e.Value = _type; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Modifier":
                    {
                        _modifier = (HPS.LinePattern.Modifier)e.Value;
                        EnableValidProperties();
                    }
                    break;
                case "Glyph":
                    {
                        _glyph = (string)e.Value;
                    }
                    break;
                case "Type":
                    {
                        _type = (T)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Set = 0,
            Modifier = 1,
            Glyph = 2,
            Type = 3
        }

        private void EnableValidProperties()
        {
            if (_modifier == HPS.LinePattern.Modifier.GlyphName)
            {
                Properties[(int)PropertyTypeIndex.Glyph].Enabled = true;
                Properties[(int)PropertyTypeIndex.Type].Enabled = false;
            }
            else if (_modifier == HPS.LinePattern.Modifier.Enumerated)
            {
                Properties[(int)PropertyTypeIndex.Glyph].Enabled = false;
                Properties[(int)PropertyTypeIndex.Type].Enabled = true;
            }
        }

        private void UpdateKit()
        {
            if (set)
            {
                if (_modifier == HPS.LinePattern.Modifier.GlyphName)
                    setCapOrJoinGlyph(_glyph);
                else if (_modifier == HPS.LinePattern.Modifier.Enumerated)
                    setCapOrJoinEnum(_type);
            }
            else
                unsetCapOrJoin();
            OnChildChanged();
        }
    }

    public class LinePatternOptionsKitStartCapProperty : LinePatternOptionsKitCapOrJoinProperty<HPS.LinePattern.Cap>
    {
        public LinePatternOptionsKitStartCapProperty(
            NestedProperty owner,
            HPS.LinePatternOptionsKit kit)
            : base(owner, kit.ShowStartCap, kit.SetStartCap, kit.SetStartCap, kit.UnsetStartCap, HPS.LinePattern.Cap.Butt)
        { }
    }

    public class LinePatternOptionsKitEndCapProperty : LinePatternOptionsKitCapOrJoinProperty<HPS.LinePattern.Cap>
    {
        public LinePatternOptionsKitEndCapProperty(
            NestedProperty owner,
            HPS.LinePatternOptionsKit kit)
            : base(owner, kit.ShowEndCap, kit.SetEndCap, kit.SetEndCap, kit.UnsetEndCap, HPS.LinePattern.Cap.Butt)
        { }
    }

    [ExpandableObject]
    public class LinePatternOptionsKitInnerCapProperty : NestedProperty
    {
        private HPS.LinePatternOptionsKit kit;
        private bool set;
        private HPS.LinePattern.Cap _type;

        public LinePatternOptionsKitInnerCapProperty(
            NestedProperty owner,
            HPS.LinePatternOptionsKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowInnerCap(out _type);
            if (!set)
            {
                _type = HPS.LinePattern.Cap.Butt;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Type", typeof(HPS.LinePattern.Cap), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Type": e.Value = _type; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Type":
                    {
                        _type = (HPS.LinePattern.Cap)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetInnerCap(_type);
            else
                kit.UnsetInnerCap();
            OnChildChanged();
        }
    }

    public class LinePatternOptionsKitJoinProperty : LinePatternOptionsKitCapOrJoinProperty<HPS.LinePattern.Join>
    {
        public LinePatternOptionsKitJoinProperty(
            NestedProperty owner,
            HPS.LinePatternOptionsKit kit)
            : base(owner, kit.ShowJoin, kit.SetJoin, kit.SetJoin, kit.UnsetJoin, HPS.LinePattern.Join.Mitre)
        { }
    }

    public class LinePatternOptionsKitProperty : NestedProperty
    {
        private LinePatternOptionsKitStartCapProperty startCap;
        private LinePatternOptionsKitEndCapProperty endCap;
        private LinePatternOptionsKitInnerCapProperty innerCap;
        private LinePatternOptionsKitJoinProperty join;

        public LinePatternOptionsKitProperty(
            NestedProperty owner,
            HPS.LinePatternOptionsKit kit)
            : base(owner)
        {
            GetValue += Get;
            startCap = new LinePatternOptionsKitStartCapProperty(this, kit);
            Properties.Add(new PropertySpec("StartCap", typeof(LinePatternOptionsKitStartCapProperty), null, expandable: true));
            endCap = new LinePatternOptionsKitEndCapProperty(this, kit);
            Properties.Add(new PropertySpec("EndCap", typeof(LinePatternOptionsKitEndCapProperty), null, expandable: true));
            innerCap = new LinePatternOptionsKitInnerCapProperty(this, kit);
            Properties.Add(new PropertySpec("InnerCap", typeof(LinePatternOptionsKitInnerCapProperty), null, expandable: true));
            join = new LinePatternOptionsKitJoinProperty(this, kit);
            Properties.Add(new PropertySpec("Join", typeof(LinePatternOptionsKitJoinProperty), null, expandable: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "StartCap": e.Value = startCap; break;
                case "EndCap": e.Value = endCap; break;
                case "InnerCap": e.Value = innerCap; break;
                case "Join": e.Value = join; break;
            }
        }
    }
    #endregion

    #region LineAttributeKit
    [ExpandableObject]
    public class LineAttributeKitPatternProperty : NestedProperty
    {
        private HPS.LineAttributeKit kit;
        private bool set;
        private string _pattern;
        private HPS.LinePatternOptionsKit _options;

        private LinePatternOptionsKitProperty _optionsProperty;

        public LineAttributeKitPatternProperty(
            HPS.LineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowPattern(out _pattern, out _options);
            if (!set)
            {
                _pattern = "pattern";
                _options = new HPS.LinePatternOptionsKit();
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Pattern", typeof(string), null, expandable: false, triggersRefresh: false));
            _optionsProperty = new LinePatternOptionsKitProperty(this, _options);
            Properties.Add(new PropertySpec("Options", typeof(LinePatternOptionsKitProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Pattern": e.Value = _pattern; break;
                case "Options": e.Value = _optionsProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Pattern":
                    {
                        _pattern = (string)e.Value;
                    }
                    break;
                case "Options":
                    {
                        // nothing to do
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetPattern(_pattern, _options);
            else
                kit.UnsetPattern();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    [ExpandableObject]
    public class LineAttributeKitWeightProperty : NestedProperty
    {
        private HPS.LineAttributeKit kit;
        private bool set;
        private float _weight;
        private HPS.Line.SizeUnits _units;

        public LineAttributeKitWeightProperty(
            HPS.LineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowWeight(out _weight, out _units);
            if (!set)
            {
                _weight = 1.0f;
                _units = HPS.Line.SizeUnits.ScaleFactor;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Weight", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Line.SizeUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Weight": e.Value = _weight; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Weight":
                    {
                        _weight = (float)e.Value;
                    }
                    break;
                case "Units":
                    {
                        _units = (HPS.Line.SizeUnits)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetWeight(_weight, _units);
            else
                kit.UnsetWeight();
        }
    }

    [DisplayName("LineAttribute")]
    public class LineAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.LineAttributeKit kit;

        public LineAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowLineAttribute(out kit);

            Pattern = new LineAttributeKitPatternProperty(kit);
            Weight = new LineAttributeKitWeightProperty(kit);
        }

        [PropertyOrder(0)]
        public LineAttributeKitPatternProperty Pattern { get; set; }

        [PropertyOrder(1)]
        public LineAttributeKitWeightProperty Weight { get; set; }

        public void Apply()
        {
            key.UnsetLineAttribute();
            key.SetLineAttribute(kit);
        }
    }
    #endregion

    #region SelectionOptionsKit
    [ExpandableObject]
    public class SelectionOptionsKitProximityProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private float _proximity;

        public SelectionOptionsKitProximityProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowProximity(out _proximity);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Proximity", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Proximity": e.Value = _proximity; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Proximity":
                    {
                        _proximity = (float)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetProximity(_proximity);
        }
    }

    [ExpandableObject]
    public class SelectionOptionsKitLevelProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private HPS.Selection.Level _level;

        public SelectionOptionsKitLevelProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowLevel(out _level);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Level", typeof(HPS.Selection.Level), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Level": e.Value = _level; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Level":
                    {
                        _level = (HPS.Selection.Level)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetLevel(_level);
        }
    }

    [ExpandableObject]
    public class SelectionOptionsKitInternalLimitProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private ulong _limit;

        public SelectionOptionsKitInternalLimitProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowInternalLimit(out _limit);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Limit", typeof(ulong), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Limit": e.Value = _limit; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Limit":
                    {
                        _limit = (ulong)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetInternalLimit(_limit);
        }
    }

    [ExpandableObject]
    public class SelectionOptionsKitRelatedLimitProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private ulong _limit;

        public SelectionOptionsKitRelatedLimitProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowRelatedLimit(out _limit);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Limit", typeof(ulong), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Limit": e.Value = _limit; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Limit":
                    {
                        _limit = (ulong)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetRelatedLimit(_limit);
        }
    }

    [ExpandableObject]
    public class SelectionOptionsKitSortingProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private HPS.Selection.Sorting _sorting;

        public SelectionOptionsKitSortingProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowSorting(out _sorting);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Sorting", typeof(HPS.Selection.Sorting), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Sorting": e.Value = _sorting; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Sorting":
                    {
                        _sorting = (HPS.Selection.Sorting)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetSorting(_sorting);
        }
    }

    [ExpandableObject]
    public class SelectionOptionsKitAlgorithmProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private HPS.Selection.Algorithm _algorithm;

        public SelectionOptionsKitAlgorithmProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowAlgorithm(out _algorithm);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Algorithm", typeof(HPS.Selection.Algorithm), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Algorithm": e.Value = _algorithm; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Algorithm":
                    {
                        _algorithm = (HPS.Selection.Algorithm)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetAlgorithm(_algorithm);
        }
    }

    [ExpandableObject]
    public class SelectionOptionsKitGranularityProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private HPS.Selection.Granularity _granularity;

        public SelectionOptionsKitGranularityProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowGranularity(out _granularity);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Granularity", typeof(HPS.Selection.Granularity), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Granularity": e.Value = _granularity; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Granularity":
                    {
                        _granularity = (HPS.Selection.Granularity)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetGranularity(_granularity);
        }
    }

    [ExpandableObject]
    public class SelectionOptionsKitExtentCullingRespectedProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private bool _state;

        public SelectionOptionsKitExtentCullingRespectedProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowExtentCullingRespected(out _state);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetExtentCullingRespected(_state);
        }
    }

    [ExpandableObject]
    public class SelectionOptionsKitDeferralExtentCullingRespectedProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private bool _state;

        public SelectionOptionsKitDeferralExtentCullingRespectedProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowDeferralExtentCullingRespected(out _state);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetDeferralExtentCullingRespected(_state);
        }
    }

    [ExpandableObject]
    public class SelectionOptionsKitFrustumCullingRespectedProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private bool _state;

        public SelectionOptionsKitFrustumCullingRespectedProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowFrustumCullingRespected(out _state);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetFrustumCullingRespected(_state);
        }
    }

    [ExpandableObject]
    public class SelectionOptionsKitVectorCullingRespectedProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private bool _state;

        public SelectionOptionsKitVectorCullingRespectedProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowVectorCullingRespected(out _state);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetVectorCullingRespected(_state);
        }
    }

    [ExpandableObject]
    public class SelectionOptionsKitSelectabilityProperty : NestedProperty
    {
        private HPS.SelectionOptionsKit kit;
        private HPS.SelectabilityKit _selectability;

        public SelectionOptionsKitSelectabilityProperty(
            HPS.SelectionOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowSelectability(out _selectability);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Selectability", typeof(HPS.SelectabilityKit), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Selectability": e.Value = _selectability; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Selectability":
                    {
                        _selectability = (HPS.SelectabilityKit)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetSelectability(_selectability);
        }
    }

    [DisplayName("SelectionOptions")]
    public class SelectionOptionsKitProperty : IRootProperty
    {
        private HPS.WindowKey key;
        private HPS.SelectionOptionsKit kit;

        public SelectionOptionsKitProperty(
            HPS.WindowKey key)
        {
            this.key = key;
            this.key.ShowSelectionOptions(out kit);

            Proximity = new SelectionOptionsKitProximityProperty(kit);
            Level = new SelectionOptionsKitLevelProperty(kit);
            InternalLimit = new SelectionOptionsKitInternalLimitProperty(kit);
            RelatedLimit = new SelectionOptionsKitRelatedLimitProperty(kit);
            Sorting = new SelectionOptionsKitSortingProperty(kit);
            Algorithm = new SelectionOptionsKitAlgorithmProperty(kit);
            Granularity = new SelectionOptionsKitGranularityProperty(kit);
            ExtentCullingRespected = new SelectionOptionsKitExtentCullingRespectedProperty(kit);
            DeferralExtentCullingRespected = new SelectionOptionsKitDeferralExtentCullingRespectedProperty(kit);
            FrustumCullingRespected = new SelectionOptionsKitFrustumCullingRespectedProperty(kit);
            VectorCullingRespected = new SelectionOptionsKitVectorCullingRespectedProperty(kit);
        }

        [PropertyOrder(0)]
        public SelectionOptionsKitProximityProperty Proximity { get; set; }

        [PropertyOrder(1)]
        public SelectionOptionsKitLevelProperty Level { get; set; }

        [PropertyOrder(2)]
        public SelectionOptionsKitInternalLimitProperty InternalLimit { get; set; }

        [PropertyOrder(3)]
        public SelectionOptionsKitRelatedLimitProperty RelatedLimit { get; set; }

        [PropertyOrder(4)]
        public SelectionOptionsKitSortingProperty Sorting { get; set; }

        [PropertyOrder(5)]
        public SelectionOptionsKitAlgorithmProperty Algorithm { get; set; }

        [PropertyOrder(6)]
        public SelectionOptionsKitGranularityProperty Granularity { get; set; }

        [PropertyOrder(7)]
        public SelectionOptionsKitExtentCullingRespectedProperty ExtentCullingRespected { get; set; }

        [PropertyOrder(8)]
        public SelectionOptionsKitDeferralExtentCullingRespectedProperty DeferralExtentCullingRespected { get; set; }

        [PropertyOrder(9)]
        public SelectionOptionsKitFrustumCullingRespectedProperty FrustumCullingRespected { get; set; }

        [PropertyOrder(10)]
        public SelectionOptionsKitVectorCullingRespectedProperty VectorCullingRespected { get; set; }

        public void Apply()
        {
            key.SetSelectionOptions(kit);
        }
    }
    #endregion

    #region AttributeLockKit
    [ExpandableObject]
    public class SingleAttributeLockProperty : NestedProperty
    {
        private int index;
        private HPS.AttributeLock.Type[] types;
        private bool[] states;

        public SingleAttributeLockProperty(
            NestedProperty owner,
            int index,
            HPS.AttributeLock.Type[] types,
            bool[] states)
            : base(owner)
        {
            this.index = index;
            this.types = types;
            this.states = states;

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Type", typeof(HPS.AttributeLock.Type), null, expandable: false));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type": e.Value = types[index]; break;
                case "State": e.Value = states[index]; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type":
                    {
                        types[index] = (HPS.AttributeLock.Type)e.Value;
                    }
                    break;
                case "State":
                    {
                        states[index] = (bool)e.Value;
                    }
                    break;
            }
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class AttributeLockKitLockArrayProperty : ArrayProperty
    {
        private int count;
        private HPS.AttributeLock.Type[] types;
        private bool[] states;

        private ArrayList lockProperties;

        public AttributeLockKitLockArrayProperty(
            NestedProperty owner,
            HPS.AttributeLock.Type[] types,
            bool[] states)
            : base(owner, "Lock")
        {
            this.types = types;
            this.states = states;

            count = types.Length;
            lockProperties = new ArrayList();

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref types, newCount, HPS.AttributeLock.Type.Everything);
            PropertyUtilities.Resize(ref states, newCount, false);
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                lockProperties.Add(new SingleAttributeLockProperty(this, i, types, states));
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(SingleAttributeLockProperty), null, expandable: true));
            }
        }

        protected override void DeleteProperties(
            int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; --i)
            {
                Properties.RemoveAt(i + 1);
                lockProperties.RemoveAt(i);
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = lockProperties[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
            }
            OnChildChanged();
        }

        public HPS.AttributeLock.Type[] Types
        {
            get { return types; }
        }

        public bool[] States
        {
            get { return states; }
        }
    }

    [ExpandableObject]
    public class AttributeLockKitLockOrOverrideProperty : NestedProperty
    {
        public delegate bool ShowLocks(out HPS.AttributeLock.Type[] types, out bool[] states);
        public delegate HPS.AttributeLockKit SetLocks(HPS.AttributeLock.Type[] types, bool[] states);
        public delegate HPS.AttributeLockKit UnsetLocks();

        private SetLocks setLocks;
        private UnsetLocks unsetLocks;
        private bool set;

        private AttributeLockKitLockArrayProperty locksProperty;

        public AttributeLockKitLockOrOverrideProperty(
            ShowLocks showLocks,
            SetLocks setLocks,
            UnsetLocks unsetLocks)
            : base(null)
        {
            this.setLocks = setLocks;
            this.unsetLocks = unsetLocks;

            HPS.AttributeLock.Type[] types;
            bool[] states;
            set = showLocks(out types, out states);
            if (!set)
            {
                types = new HPS.AttributeLock.Type[] { HPS.AttributeLock.Type.Everything };
                states = new bool[] { false };
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            locksProperty = new AttributeLockKitLockArrayProperty(this, types, states);
            Properties.Add(new PropertySpec("Locks", typeof(AttributeLockKitLockArrayProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Locks": e.Value = locksProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                setLocks(locksProperty.Types, locksProperty.States);
            else
                unsetLocks();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    [DisplayName("AttributeLock")]
    public class AttributeLockKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.AttributeLockKit kit;

        public AttributeLockKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowAttributeLock(out kit);

            Lock = new AttributeLockKitLockOrOverrideProperty(kit.ShowLock, kit.SetLock, kit.UnsetLock);
            SubsegmentLockOverride = new AttributeLockKitLockOrOverrideProperty(kit.ShowSubsegmentLockOverride, kit.SetSubsegmentLockOverride, kit.UnsetSubsegmentLockOverride);
        }

        [PropertyOrder(0)]
        public AttributeLockKitLockOrOverrideProperty Lock { get; set; }

        [PropertyOrder(1)]
        public AttributeLockKitLockOrOverrideProperty SubsegmentLockOverride { get; set; }

        public void Apply()
        {
            key.UnsetAttributeLock();
            key.SetAttributeLock(kit);
        }
    }
    #endregion

    #region SegmentKey
    [ExpandableObject]
    public class KeyMatrixProperty : NestedProperty
    {
        public delegate bool ShowMatrix(out HPS.MatrixKit kit);

        private HPS.MatrixKit kit;
        private bool set;

        private MatrixKitProperty matrixProperty;

        public KeyMatrixProperty(
            ShowMatrix showMatrix)
            : base(null)
        {
            set = showMatrix(out kit);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            matrixProperty = new MatrixKitProperty(this, kit);
            Properties.Add(new PropertySpec("Elements", typeof(MatrixKitProperty), null, expandable: true));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Elements": e.Value = matrixProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;

                case "Elements":
                    {
                        // nothing to do
                    }
                    break;
            }
        }

        public bool IsSet
        {
            get { return set; }
        }

        public HPS.MatrixKit Kit
        {
            get { return kit; }
        }
    }

    [DisplayName("ModellingMatrix")]
    public class ReferenceKeyModellingMatrixProperty : IRootProperty
    {
        private HPS.ReferenceKey key;

        public ReferenceKeyModellingMatrixProperty(
            HPS.ReferenceKey key)
        {
            this.key = key;

            Matrix = new KeyMatrixProperty(this.key.ShowModellingMatrix);
        }

        public KeyMatrixProperty Matrix { get; set; }

        public void Apply()
        {
            if (Matrix.IsSet)
                key.SetModellingMatrix(Matrix.Kit);
            else
                key.UnsetModellingMatrix();
        }
    }

    [DisplayName("ModellingMatrix")]
    public class SegmentKeyModellingMatrixProperty : IRootProperty
    {
        private HPS.SegmentKey key;

        public SegmentKeyModellingMatrixProperty(
            HPS.SegmentKey key)
        {
            this.key = key;

            Matrix = new KeyMatrixProperty(this.key.ShowModellingMatrix);
        }

        public KeyMatrixProperty Matrix { get; set; }

        public void Apply()
        {
            if (Matrix.IsSet)
                key.SetModellingMatrix(Matrix.Kit);
            else
                key.UnsetModellingMatrix();
        }
    }

    [DisplayName("TextureMatrix")]
    public class SegmentKeyTextureMatrixProperty : IRootProperty
    {
        private HPS.SegmentKey key;

        public SegmentKeyTextureMatrixProperty(
            HPS.SegmentKey key)
        {
            this.key = key;

            Matrix = new KeyMatrixProperty(this.key.ShowTextureMatrix);
        }

        public KeyMatrixProperty Matrix { get; set; }

        public void Apply()
        {
            if (Matrix.IsSet)
                key.SetTextureMatrix(Matrix.Kit);
            else
                key.UnsetTextureMatrix();
        }
    }

    [ExpandableObject]
    public class KeyUserDataProperty : NestedProperty
    {
        public delegate bool ShowUserData(out IntPtr[] indices, out byte[][] data);

        private bool set;
        private IntPtr[] indices;
        private byte[][] data;

        private UserDataImmutableArrayProperty userDataProperty;

        public KeyUserDataProperty(
            ShowUserData showUserData)
            : base(null)
        {
            set = showUserData(out indices, out data);
            if (set)
                userDataProperty = new UserDataImmutableArrayProperty(this, indices, data);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            if (set)
                Properties.Add(new PropertySpec("User Data", typeof(UserDataImmutableArrayProperty), null, expandable: true));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "User Data": e.Value = userDataProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
            }
        }

        public bool IsSet
        {
            get { return set; }
        }
    }

    [DisplayName("User Data")]
    public class SegmentKeyUserDataProperty : IRootProperty
    {
        private HPS.SegmentKey key;

        public SegmentKeyUserDataProperty(
            HPS.SegmentKey key)
        {
            this.key = key;

            UserData = new KeyUserDataProperty(this.key.ShowUserData);
        }

        [PropertyOrder(0)]
        public KeyUserDataProperty UserData { get; set; }

        public void Apply()
        {
            if (!UserData.IsSet)
                key.UnsetAllUserData();
        }
    }

    [ExpandableObject]
    public class SegmentNameProperty
    {
        public SegmentNameProperty(
            HPS.SegmentKey key)
        {
            Value = key.Name();
        }

        public override string ToString()
        {
            return "";
        }

        [PropertyOrder(0)]
        public string Value { get; set; }
    }

    [ExpandableObject]
    public class NetBoundingProperty : DynamicProperty
    {
        private ImmutableSimpleSphereProperty sphereProperty;
        private ImmutableSimpleCuboidProperty cuboidProperty;

        public NetBoundingProperty(
            HPS.KeyPath keyPath)
        {
            GetValue += Get;

            HPS.BoundingKit bounding;
            if (keyPath.ShowNetBounding(out bounding))
            {
                HPS.SimpleSphere sphere;
                HPS.SimpleCuboid cuboid;
                if (bounding.ShowVolume(out sphere, out cuboid))
                {
                    sphereProperty = new ImmutableSimpleSphereProperty(sphere);
                    Properties.Add(new PropertySpec("Sphere", typeof(ImmutableSimpleSphereProperty), null, expandable: true));
                    cuboidProperty = new ImmutableSimpleCuboidProperty(cuboid);
                    Properties.Add(new PropertySpec("Cuboid", typeof(ImmutableSimpleCuboidProperty), null, expandable: true));
                }
            }

            if (Properties.Count == 0)
                Properties.Add(new PropertySpec("None", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        public override string ToString()
        {
            return "";
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Sphere": e.Value = sphereProperty; break;
                case "Cuboid": e.Value = cuboidProperty; break;
                case "None": e.Value = ""; break;
            }
        }
    }

    [ExpandableObject]
    public class SegmentContentsCountProperty : DynamicProperty
    {
        private struct SearchTypeData
        {
            public string name;
            public ulong count;

            public SearchTypeData(
                string name,
                ulong count = 0)
            {
                this.name = name;
                this.count = count;
            }
        }

        private OrderedDictionary countDict;

        public SegmentContentsCountProperty(
            HPS.SegmentKey key)
        {
            GetValue += Get;

            CountContents(key);
            foreach (DictionaryEntry de in countDict)
            {
                var typeData = (SearchTypeData)de.Value;
                if (typeData.count > 0)
                    Properties.Add(new PropertySpec(typeData.name, typeof(ulong), null, expandable: false, triggersRefresh: false, readOnly: true));
            }

            if (Properties.Count == 0)
                Properties.Add(new PropertySpec("None", typeof(string), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void CountContents(
            HPS.SegmentKey key)
        {
            countDict = new OrderedDictionary();
            HPS.Search.Type[] types =
            {
                HPS.Search.Type.Segment,
                HPS.Search.Type.Include,
                HPS.Search.Type.CuttingSection,
                HPS.Search.Type.Shell,
                HPS.Search.Type.Mesh,
                HPS.Search.Type.Grid,
                HPS.Search.Type.NURBSSurface,
                HPS.Search.Type.Cylinder,
                HPS.Search.Type.Sphere,
                HPS.Search.Type.Polygon,
                HPS.Search.Type.Circle,
                HPS.Search.Type.CircularWedge,
                HPS.Search.Type.Ellipse,
                HPS.Search.Type.Line,
                HPS.Search.Type.NURBSCurve,
                HPS.Search.Type.CircularArc,
                HPS.Search.Type.EllipticalArc,
                HPS.Search.Type.InfiniteLine,
                HPS.Search.Type.InfiniteRay,
                HPS.Search.Type.Marker,
                HPS.Search.Type.Text,
                HPS.Search.Type.Reference,
                HPS.Search.Type.DistantLight,
                HPS.Search.Type.Spotlight
            };
            HPS.SearchResults searchResults;
            if (key.Find(types, HPS.Search.Space.SegmentOnly, out searchResults) > 0)
            {
                countDict.Add(HPS.Search.Type.Segment, new SearchTypeData("Subsegments"));
                countDict.Add(HPS.Search.Type.Include, new SearchTypeData("Includes"));
                countDict.Add(HPS.Search.Type.CuttingSection, new SearchTypeData("Cutting Sections"));
                countDict.Add(HPS.Search.Type.Shell, new SearchTypeData("Shells"));
                countDict.Add(HPS.Search.Type.Mesh, new SearchTypeData("Meshes"));
                countDict.Add(HPS.Search.Type.Grid, new SearchTypeData("Grids"));
                countDict.Add(HPS.Search.Type.NURBSSurface, new SearchTypeData("NURBS Surfaces"));
                countDict.Add(HPS.Search.Type.Cylinder, new SearchTypeData("Cylinders"));
                countDict.Add(HPS.Search.Type.Sphere, new SearchTypeData("Spheres"));
                countDict.Add(HPS.Search.Type.Polygon, new SearchTypeData("Polygons"));
                countDict.Add(HPS.Search.Type.Circle, new SearchTypeData("Circles"));
                countDict.Add(HPS.Search.Type.CircularWedge, new SearchTypeData("Circular Wedges"));
                countDict.Add(HPS.Search.Type.Ellipse, new SearchTypeData("Ellipses"));
                countDict.Add(HPS.Search.Type.Line, new SearchTypeData("Lines"));
                countDict.Add(HPS.Search.Type.NURBSCurve, new SearchTypeData("NURBS Curves"));
                countDict.Add(HPS.Search.Type.CircularArc, new SearchTypeData("Circular Arcs"));
                countDict.Add(HPS.Search.Type.EllipticalArc, new SearchTypeData("Elliptical Arcs"));
                countDict.Add(HPS.Search.Type.InfiniteLine, new SearchTypeData("Infinite Lines"));
                countDict.Add(HPS.Search.Type.InfiniteRay, new SearchTypeData("Infinite Rays"));
                countDict.Add(HPS.Search.Type.Marker, new SearchTypeData("Markers"));
                countDict.Add(HPS.Search.Type.Text, new SearchTypeData("Text"));
                countDict.Add(HPS.Search.Type.Reference, new SearchTypeData("References"));
                countDict.Add(HPS.Search.Type.DistantLight, new SearchTypeData("Distant Lights"));
                countDict.Add(HPS.Search.Type.Spotlight, new SearchTypeData("Spotlights"));

                HPS.SearchResultsIterator it = searchResults.GetIterator();
                while (it.IsValid())
                {
                    HPS.Search.Type[] resultTypes = it.GetResultTypes();
                    var typeData = (SearchTypeData)countDict[resultTypes[0]];
                    typeData.count += 1;
                    countDict[resultTypes[0]] = typeData;
                    it.Next();
                }
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Subsegments": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Segment]).count; break;
                case "Includes": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Include]).count; break;
                case "Cutting Sections": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.CuttingSection]).count; break;
                case "Shells": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Shell]).count; break;
                case "Meshes": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Mesh]).count; break;
                case "Grids": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Grid]).count; break;
                case "NURBS Surfaces": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.NURBSSurface]).count; break;
                case "Cylinders": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Cylinder]).count; break;
                case "Spheres": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Sphere]).count; break;
                case "Polygons": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Polygon]).count; break;
                case "Circles": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Circle]).count; break;
                case "Circular Wedges": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.CircularWedge]).count; break;
                case "Ellipses": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Ellipse]).count; break;
                case "Lines": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Line]).count; break;
                case "NURBS Curves": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.NURBSCurve]).count; break;
                case "Circular Arcs": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.CircularArc]).count; break;
                case "Elliptical Arcs": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.EllipticalArc]).count; break;
                case "Infinite Lines": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.InfiniteLine]).count; break;
                case "Infinite Rays": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.InfiniteRay]).count; break;
                case "Markers": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Marker]).count; break;
                case "Text": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Text]).count; break;
                case "References": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Reference]).count; break;
                case "Distant Lights": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.DistantLight]).count; break;
                case "Spotlights": e.Value = ((SearchTypeData)countDict[HPS.Search.Type.Spotlight]).count; break;
                case "None": e.Value = ""; break;
            }
        }
    }

    [DisplayName("Segment")]
    public class SegmentKeyProperty : IRootProperty
    {
        private HPS.SegmentKey key;

        public SegmentKeyProperty(
            HPS.SegmentKey key,
            HPS.KeyPath keyPath)
        {
            this.key = key;

            Name = new SegmentNameProperty(this.key);
            NetBounding = new NetBoundingProperty(keyPath);
            Contents = new SegmentContentsCountProperty(this.key);
        }

        [PropertyOrder(0)]
        public SegmentNameProperty Name { get; set; }

        [PropertyOrder(1)]
        public NetBoundingProperty NetBounding { get; set; }

        [PropertyOrder(2)]
        public SegmentContentsCountProperty Contents { get; set; }

        public void Apply()
        {
            if (key.Name() != Name.Value)
                key.SetName(Name.Value);
        }
    }

    [DisplayName("Priority")]
    public class SegmentKeyPriorityProperty : NestedProperty, IRootProperty
    {
        private HPS.SegmentKey key;
        private bool set;
        private int priority;

        public SegmentKeyPriorityProperty(
            HPS.SegmentKey key)
            : base(null)
        {
            this.key = key;

            set = this.key.ShowPriority(out priority);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Priority", typeof(int), null, expandable: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Priority": e.Value = priority; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Priority":
                    {
                        priority = (int)e.Value;
                    }
                    break;
            }
        }

        public void Apply()
        {
            if (set)
                key.SetPriority(priority);
            else
                key.UnsetPriority();
        }
    }

    [ExpandableObject]
    public class MaterialPaletteProperty : NestedProperty
    {
        private bool set;
        private string materialPalette;

        public MaterialPaletteProperty(
            HPS.SegmentKey key)
            : base(null)
        {
            set = key.ShowMaterialPalette(out materialPalette);
            if (!set)
                materialPalette = "name";

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Name", typeof(string), null, expandable: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Name": e.Value = materialPalette; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;

                case "Name":
                    {
                        materialPalette = (string)e.Value;
                    }
                    break;
            }
        }

        public bool IsSet
        {
            get { return set; }
        }

        public string MaterialPalette
        {
            get { return materialPalette; }
        }
    }

    [DisplayName("Material Palette")]
    public class SegmentKeyMaterialPaletteProperty : IRootProperty
    {
        private HPS.SegmentKey key;

        public SegmentKeyMaterialPaletteProperty(
            HPS.SegmentKey key)
        {
            this.key = key;

            MaterialPalette = new MaterialPaletteProperty(this.key);
        }

        [PropertyOrder(0)]
        public MaterialPaletteProperty MaterialPalette { get; set; }

        public void Apply()
        {
            if (MaterialPalette.IsSet)
                key.SetMaterialPalette(MaterialPalette.MaterialPalette);
            else
                key.UnsetMaterialPalette();
        }
    }

    public class ConditionsArrayProperty : ArrayProperty
    {
        private int count;
        private string[] conditions;

        public ConditionsArrayProperty(
            string[] conditions)
            : base(null, "Condition")
        {
            this.conditions = conditions;

            count = conditions.Length;

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref conditions, newCount, "condition");
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(string), null, expandable: false));
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = conditions[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
                default:
                    {
                        conditions[GetIndexFromName(e)] = (string)e.Value;
                    }
                    break;
            }
            OnChildChanged();
        }

        public string[] Conditions
        {
            get { return conditions; }
        }
    }

    public class SegmentKeyConditionProperty : NestedProperty, IRootProperty
    {
        private HPS.SegmentKey key;
        private bool set;

        private ConditionsArrayProperty conditionsProperty;

        public SegmentKeyConditionProperty(
            HPS.SegmentKey key)
            : base(null)
        {
            this.key = key;

            string[] conditions;
            set = this.key.ShowConditions(out conditions);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            conditionsProperty = new ConditionsArrayProperty(conditions);
            Properties.Add(new PropertySpec("Conditions", typeof(ConditionsArrayProperty), null, expandable: true));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Conditions": e.Value = conditionsProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
            }
        }

        public void Apply()
        {
            if (set)
                key.SetConditions(conditionsProperty.Conditions);
            else
                key.UnsetConditions();
        }
    }
    #endregion

    #region AttributeFilter
    [ExpandableObject]
    public class AttributeFilterArrayProperty : ArrayProperty
    {
        private int count;
        private HPS.AttributeLock.Type[] types;

        public AttributeFilterArrayProperty(
            NestedProperty owner,
            HPS.AttributeLock.Type[] types)
            : base(owner, "Type")
        {
            this.types = types;

            count = this.types.Length;

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref types, newCount, HPS.AttributeLock.Type.Everything);
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(HPS.AttributeLock.Type), null, expandable: false));
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = types[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
                default:
                    {
                        types[GetIndexFromName(e)] = (HPS.AttributeLock.Type)e.Value;
                    }
                    break;
            }
            OnChildChanged();
        }

        public HPS.AttributeLock.Type[] Types
        {
            get { return types; }
        }
    }

    public class AttributeFilterProperty<T> : NestedProperty, IRootProperty
    {
        public delegate bool ShowFilter(out HPS.AttributeLock.Type[] types);
        public delegate T SetFilter(HPS.AttributeLock.Type[] types);
        public delegate T UnsetFilter(HPS.AttributeLock.Type[] types);

        private SetFilter setFilter;
        private UnsetFilter unsetFilter;
        private bool set;
        private HPS.AttributeLock.Type[] originalTypes;

        private AttributeFilterArrayProperty filterProperty;

        public AttributeFilterProperty(
            ShowFilter showFilter,
            SetFilter setFilter,
            UnsetFilter unsetFilter)
            : base(null)
        {
            this.setFilter = setFilter;
            this.unsetFilter = unsetFilter;

            HPS.AttributeLock.Type[] types;
            set = showFilter(out types);
            if (set)
                originalTypes = types;
            else
                types = new HPS.AttributeLock.Type[] { HPS.AttributeLock.Type.Everything };

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            filterProperty = new AttributeFilterArrayProperty(this, types);
            Properties.Add(new PropertySpec("Filter", typeof(AttributeFilterArrayProperty), null, expandable: true));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Filter": e.Value = filterProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
            }
        }

        public void Apply()
        {
            if (set)
            {
                unsetFilter(originalTypes);
                setFilter(filterProperty.Types);
            }
            else
                unsetFilter(originalTypes);
        }
    }

    [DisplayName("Attribute Filter")]
    public class StyleKeyAttributeFilterProperty : AttributeFilterProperty<HPS.StyleKey>
    {
        public StyleKeyAttributeFilterProperty(
            HPS.StyleKey key)
            : base(key.ShowFilter, key.SetFilter, key.UnsetFilter)
        { }
    }

    [DisplayName("Attribute Filter")]
    public class IncludeKeyAttributeFilterProperty : AttributeFilterProperty<HPS.IncludeKey>
    {
        public IncludeKeyAttributeFilterProperty(
            HPS.IncludeKey key)
            : base(key.ShowFilter, key.SetFilter, key.UnsetFilter)
        { }
    }
    #endregion

    #region WindowInfoKit
    [ExpandableObject]
    public class WindowInfoKitPhysicalPixelsProperty : NestedProperty
    {
        private uint _width;
        private uint _height;

        public WindowInfoKitPhysicalPixelsProperty(
            HPS.WindowInfoKit kit)
            : base(null)
        {
            kit.ShowPhysicalPixels(out _width, out _height);

            GetValue += Get;
            Properties.Add(new PropertySpec("Width", typeof(uint), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Height", typeof(uint), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Width": e.Value = _width; break;
                case "Height": e.Value = _height; break;
            }
        }
    }

    [ExpandableObject]
    public class WindowInfoKitPhysicalSizeProperty : NestedProperty
    {
        private float _width;
        private float _height;

        public WindowInfoKitPhysicalSizeProperty(
            HPS.WindowInfoKit kit)
            : base(null)
        {
            kit.ShowPhysicalSize(out _width, out _height);

            GetValue += Get;
            Properties.Add(new PropertySpec("Width", typeof(float), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Height", typeof(float), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Width": e.Value = _width; break;
                case "Height": e.Value = _height; break;
            }
        }
    }

    [ExpandableObject]
    public class WindowInfoKitWindowPixelsProperty : NestedProperty
    {
        private uint _width;
        private uint _height;

        public WindowInfoKitWindowPixelsProperty(
            HPS.WindowInfoKit kit)
            : base(null)
        {
            kit.ShowWindowPixels(out _width, out _height);

            GetValue += Get;
            Properties.Add(new PropertySpec("Width", typeof(uint), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Height", typeof(uint), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Width": e.Value = _width; break;
                case "Height": e.Value = _height; break;
            }
        }
    }

    [ExpandableObject]
    public class WindowInfoKitWindowSizeProperty : NestedProperty
    {
        private float _width;
        private float _height;

        public WindowInfoKitWindowSizeProperty(
            HPS.WindowInfoKit kit)
            : base(null)
        {
            kit.ShowWindowSize(out _width, out _height);

            GetValue += Get;
            Properties.Add(new PropertySpec("Width", typeof(float), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Height", typeof(float), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Width": e.Value = _width; break;
                case "Height": e.Value = _height; break;
            }
        }
    }

    [ExpandableObject]
    public class WindowInfoKitResolutionProperty : NestedProperty
    {
        private float _horizontal;
        private float _vertical;

        public WindowInfoKitResolutionProperty(
            HPS.WindowInfoKit kit)
            : base(null)
        {
            kit.ShowResolution(out _horizontal, out _vertical);

            GetValue += Get;
            Properties.Add(new PropertySpec("Horizontal", typeof(float), null, expandable: false, triggersRefresh: false, readOnly: true));
            Properties.Add(new PropertySpec("Vertical", typeof(float), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Horizontal": e.Value = _horizontal; break;
                case "Vertical": e.Value = _vertical; break;
            }
        }
    }

    [ExpandableObject]
    public class WindowInfoKitWindowAspectRatioProperty : NestedProperty
    {
        private float _window_aspect;

        public WindowInfoKitWindowAspectRatioProperty(
            HPS.WindowInfoKit kit)
            : base(null)
        {
            kit.ShowWindowAspectRatio(out _window_aspect);

            GetValue += Get;
            Properties.Add(new PropertySpec("Window Aspect", typeof(float), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Window Aspect": e.Value = _window_aspect; break;
            }
        }
    }

    [ExpandableObject]
    public class WindowInfoKitPixelAspectRatioProperty : NestedProperty
    {
        private float _pixel_aspect;

        public WindowInfoKitPixelAspectRatioProperty(
            HPS.WindowInfoKit kit)
            : base(null)
        {
            kit.ShowPixelAspectRatio(out _pixel_aspect);

            GetValue += Get;
            Properties.Add(new PropertySpec("Pixel Aspect", typeof(float), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Pixel Aspect": e.Value = _pixel_aspect; break;
            }
        }
    }
    #endregion

    #region ApplicationWindowKey
    [ExpandableObject]
    public class ApplicationWindowOptionsKitDriverProperty : NestedProperty
    {
        private HPS.Window.Driver _driver;

        public ApplicationWindowOptionsKitDriverProperty(
            HPS.ApplicationWindowOptionsKit kit)
            : base(null)
        {
            kit.ShowDriver(out _driver);

            GetValue += Get;
            Properties.Add(new PropertySpec("Driver", typeof(HPS.Window.Driver), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Driver": e.Value = _driver; break;
            }
        }
    }

    [ExpandableObject]
    public class ApplicationWindowOptionsKitAntiAliasCapableProperty : NestedProperty
    {
        private bool _state;
        private uint _samples;

        public ApplicationWindowOptionsKitAntiAliasCapableProperty(
            HPS.ApplicationWindowOptionsKit kit)
            : base(null)
        {
            kit.ShowAntiAliasCapable(out _state, out _samples);

            GetValue += Get;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false, readOnly: true));
            if (_state)
                Properties.Add(new PropertySpec("Samples", typeof(uint), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
                case "Samples": e.Value = _samples; break;
            }
        }
    }

    [ExpandableObject]
    public class ApplicationWindowOptionsKitPlatformDataProperty : NestedProperty
    {
        private IntPtr _platform_data;

        public ApplicationWindowOptionsKitPlatformDataProperty(
            HPS.ApplicationWindowOptionsKit kit)
            : base(null)
        {
            kit.ShowPlatformData(out _platform_data);

            GetValue += Get;
            Properties.Add(new PropertySpec("Platform Data", typeof(IntPtr), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Platform Data": e.Value = _platform_data; break;
            }
        }
    }

    [ExpandableObject]
    public class ApplicationWindowOptionsKitFramebufferRetentionProperty : NestedProperty
    {
        private bool _retain;

        public ApplicationWindowOptionsKitFramebufferRetentionProperty(
            HPS.ApplicationWindowOptionsKit kit)
            : base(null)
        {
            kit.ShowFramebufferRetention(out _retain);

            GetValue += Get;
            Properties.Add(new PropertySpec("Retain", typeof(bool), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Retain": e.Value = _retain; break;
            }
        }
    }

    [DisplayName("ApplicationWindow")]
    public class ApplicationWindowKeyProperty : IRootProperty
    {
        private HPS.ApplicationWindowKey key;
        private HPS.WindowInfoKit windowInfoKit;
        private HPS.ApplicationWindowOptionsKit applicationWindowOptionsKit;

        public ApplicationWindowKeyProperty(
            HPS.ApplicationWindowKey key)
        {
            this.key = key;
            this.key.ShowWindowInfo(out windowInfoKit);
            this.key.ShowWindowOptions(out applicationWindowOptionsKit);

            Name = new SegmentNameProperty(this.key);
            NetBounding = new NetBoundingProperty(new HPS.KeyPath(new HPS.Key[] { this.key }));
            Contents = new SegmentContentsCountProperty(this.key);

            PhysicalPixels = new WindowInfoKitPhysicalPixelsProperty(windowInfoKit);
            PhysicalSize = new WindowInfoKitPhysicalSizeProperty(windowInfoKit);
            WindowPixels = new WindowInfoKitWindowPixelsProperty(windowInfoKit);
            WindowSize = new WindowInfoKitWindowSizeProperty(windowInfoKit);
            Resolution = new WindowInfoKitResolutionProperty(windowInfoKit);
            WindowAspectRatio = new WindowInfoKitWindowAspectRatioProperty(windowInfoKit);
            PixelAspectRatio = new WindowInfoKitPixelAspectRatioProperty(windowInfoKit);

            Driver = new ApplicationWindowOptionsKitDriverProperty(applicationWindowOptionsKit);
            AntiAliasCapable = new ApplicationWindowOptionsKitAntiAliasCapableProperty(applicationWindowOptionsKit);
            PlatformData = new ApplicationWindowOptionsKitPlatformDataProperty(applicationWindowOptionsKit);
            FramebufferRetention = new ApplicationWindowOptionsKitFramebufferRetentionProperty(applicationWindowOptionsKit);
        }

        [PropertyOrder(0)]
        public SegmentNameProperty Name { get; set; }

        [PropertyOrder(1)]
        public NetBoundingProperty NetBounding { get; set; }

        [PropertyOrder(2)]
        public SegmentContentsCountProperty Contents { get; set; }

        [PropertyOrder(3)]
        public WindowInfoKitPhysicalPixelsProperty PhysicalPixels { get; set; }

        [PropertyOrder(4)]
        public WindowInfoKitPhysicalSizeProperty PhysicalSize { get; set; }

        [PropertyOrder(5)]
        public WindowInfoKitWindowPixelsProperty WindowPixels { get; set; }

        [PropertyOrder(6)]
        public WindowInfoKitWindowSizeProperty WindowSize { get; set; }

        [PropertyOrder(7)]
        public WindowInfoKitResolutionProperty Resolution { get; set; }

        [PropertyOrder(8)]
        public WindowInfoKitWindowAspectRatioProperty WindowAspectRatio { get; set; }

        [PropertyOrder(9)]
        public WindowInfoKitPixelAspectRatioProperty PixelAspectRatio { get; set; }

        [PropertyOrder(10)]
        public ApplicationWindowOptionsKitDriverProperty Driver { get; set; }

        [PropertyOrder(11)]
        public ApplicationWindowOptionsKitAntiAliasCapableProperty AntiAliasCapable { get; set; }

        [PropertyOrder(12)]
        public ApplicationWindowOptionsKitPlatformDataProperty PlatformData { get; set; }

        [PropertyOrder(13)]
        public ApplicationWindowOptionsKitFramebufferRetentionProperty FramebufferRetention { get; set; }

        public void Apply()
        {
            if (key.Name() != Name.Value)
                key.SetName(Name.Value);
        }
    }
    #endregion

    #region OffScreenWindowKey
    [ExpandableObject]
    public class OffScreenWindowOptionsKitDriverProperty : NestedProperty
    {
        private HPS.Window.Driver _driver;

        public OffScreenWindowOptionsKitDriverProperty(
            HPS.OffScreenWindowOptionsKit kit)
            : base(null)
        {
            kit.ShowDriver(out _driver);

            GetValue += Get;
            Properties.Add(new PropertySpec("Driver", typeof(HPS.Window.Driver), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Driver": e.Value = _driver; break;
            }
        }
    }

    [ExpandableObject]
    public class OffScreenWindowOptionsKitAntiAliasCapableProperty : NestedProperty
    {
        private bool _state;
        private uint _samples;

        public OffScreenWindowOptionsKitAntiAliasCapableProperty(
            HPS.OffScreenWindowOptionsKit kit)
            : base(null)
        {
            kit.ShowAntiAliasCapable(out _state, out _samples);

            GetValue += Get;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false, readOnly: true));
            if (_state)
                Properties.Add(new PropertySpec("Samples", typeof(uint), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
                case "Samples": e.Value = _samples; break;
            }
        }
    }

    [ExpandableObject]
    public class OffScreenWindowOptionsKitHardwareResidentProperty : NestedProperty
    {
        private bool _state;

        public OffScreenWindowOptionsKitHardwareResidentProperty(
            HPS.OffScreenWindowOptionsKit kit)
            : base(null)
        {
            kit.ShowHardwareResident(out _state);

            GetValue += Get;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }
    }

    [ExpandableObject]
    public class OffScreenWindowOptionsKitNativeFormatProperty : NestedProperty
    {
        private HPS.OffScreenWindowOptionsKit kit;
        private HPS.Window.ImageFormat _format;
        private float _quality;

        public OffScreenWindowOptionsKitNativeFormatProperty(
            HPS.OffScreenWindowOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowNativeFormat(out _format, out _quality);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Format", typeof(HPS.Window.ImageFormat), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Quality", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Format": e.Value = _format; break;
                case "Quality": e.Value = _quality; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Format":
                    {
                        _format = (HPS.Window.ImageFormat)e.Value;
                    }
                    break;
                case "Quality":
                    {
                        _quality = (float)e.Value;
                    }
                    break;
            }
            kit.SetNativeFormat(_format, _quality);
        }
    }

    [ExpandableObject]
    public class OffScreenWindowOptionsKitOpacityProperty : NestedProperty
    {
        private bool _state;
        private float _opacity;

        public OffScreenWindowOptionsKitOpacityProperty(
            HPS.OffScreenWindowOptionsKit kit)
            : base(null)
        {
            kit.ShowOpacity(out _state, out _opacity);

            GetValue += Get;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false, readOnly: true));
            if (_state)
                Properties.Add(new PropertySpec("Opacity", typeof(float), null, expandable: false, triggersRefresh: false, readOnly: true));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
                case "Opacity": e.Value = _opacity; break;
            }
        }
    }

    [DisplayName("OffScreenWindow")]
    public class OffScreenWindowKeyProperty : IRootProperty
    {
        private HPS.OffScreenWindowKey key;
        private HPS.WindowInfoKit windowInfoKit;
        private HPS.OffScreenWindowOptionsKit offScreenWindowOptionsKit;

        public OffScreenWindowKeyProperty(
            HPS.OffScreenWindowKey key)
        {
            this.key = key;
            this.key.ShowWindowInfo(out windowInfoKit);
            this.key.ShowWindowOptions(out offScreenWindowOptionsKit);

            Name = new SegmentNameProperty(this.key);
            NetBounding = new NetBoundingProperty(new HPS.KeyPath(new HPS.Key[] { this.key }));
            Contents = new SegmentContentsCountProperty(this.key);

            PhysicalPixels = new WindowInfoKitPhysicalPixelsProperty(windowInfoKit);
            PhysicalSize = new WindowInfoKitPhysicalSizeProperty(windowInfoKit);
            WindowPixels = new WindowInfoKitWindowPixelsProperty(windowInfoKit);
            WindowSize = new WindowInfoKitWindowSizeProperty(windowInfoKit);
            Resolution = new WindowInfoKitResolutionProperty(windowInfoKit);
            WindowAspectRatio = new WindowInfoKitWindowAspectRatioProperty(windowInfoKit);
            PixelAspectRatio = new WindowInfoKitPixelAspectRatioProperty(windowInfoKit);

            Driver = new OffScreenWindowOptionsKitDriverProperty(offScreenWindowOptionsKit);
            AntiAliasCapable = new OffScreenWindowOptionsKitAntiAliasCapableProperty(offScreenWindowOptionsKit);
            HardwareResident = new OffScreenWindowOptionsKitHardwareResidentProperty(offScreenWindowOptionsKit);
            NativeFormat = new OffScreenWindowOptionsKitNativeFormatProperty(offScreenWindowOptionsKit);
            Opacity = new OffScreenWindowOptionsKitOpacityProperty(offScreenWindowOptionsKit);
        }

        [PropertyOrder(0)]
        public SegmentNameProperty Name { get; set; }

        [PropertyOrder(1)]
        public NetBoundingProperty NetBounding { get; set; }

        [PropertyOrder(2)]
        public SegmentContentsCountProperty Contents { get; set; }

        [PropertyOrder(3)]
        public WindowInfoKitPhysicalPixelsProperty PhysicalPixels { get; set; }

        [PropertyOrder(4)]
        public WindowInfoKitPhysicalSizeProperty PhysicalSize { get; set; }

        [PropertyOrder(5)]
        public WindowInfoKitWindowPixelsProperty WindowPixels { get; set; }

        [PropertyOrder(6)]
        public WindowInfoKitWindowSizeProperty WindowSize { get; set; }

        [PropertyOrder(7)]
        public WindowInfoKitResolutionProperty Resolution { get; set; }

        [PropertyOrder(8)]
        public WindowInfoKitWindowAspectRatioProperty WindowAspectRatio { get; set; }

        [PropertyOrder(9)]
        public WindowInfoKitPixelAspectRatioProperty PixelAspectRatio { get; set; }

        [PropertyOrder(10)]
        public OffScreenWindowOptionsKitDriverProperty Driver { get; set; }

        [PropertyOrder(11)]
        public OffScreenWindowOptionsKitAntiAliasCapableProperty AntiAliasCapable { get; set; }

        [PropertyOrder(12)]
        public OffScreenWindowOptionsKitHardwareResidentProperty HardwareResident { get; set; }

        [PropertyOrder(13)]
        public OffScreenWindowOptionsKitNativeFormatProperty NativeFormat { get; set; }

        [PropertyOrder(14)]
        public OffScreenWindowOptionsKitOpacityProperty Opacity { get; set; }

        public void Apply()
        {
            if (key.Name() != Name.Value)
                key.SetName(Name.Value);

            HPS.Window.ImageFormat format;
            float quality;
            if (offScreenWindowOptionsKit.ShowNativeFormat(out format, out quality))
                key.GetWindowOptionsControl().SetNativeFormat(format, quality);
        }
    }
    #endregion

    #region VisibilityKit
    [ExpandableObject]
    public class VisibilityKitCuttingSectionsProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitCuttingSectionsProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowCuttingSections(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCuttingSections(_state);
            else
                kit.UnsetCuttingSections();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitCutEdgesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitCutEdgesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowCutEdges(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCutEdges(_state);
            else
                kit.UnsetCutEdges();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitCutFacesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitCutFacesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowCutFaces(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCutFaces(_state);
            else
                kit.UnsetCutFaces();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitWindowsProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitWindowsProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowWindows(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetWindows(_state);
            else
                kit.UnsetWindows();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitTextProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitTextProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowText(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetText(_state);
            else
                kit.UnsetText();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitLinesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitLinesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowLines(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetLines(_state);
            else
                kit.UnsetLines();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitEdgeLightsProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitEdgeLightsProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowEdgeLights(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetEdgeLights(_state);
            else
                kit.UnsetEdgeLights();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitMarkerLightsProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitMarkerLightsProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowMarkerLights(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMarkerLights(_state);
            else
                kit.UnsetMarkerLights();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitFaceLightsProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitFaceLightsProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowFaceLights(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetFaceLights(_state);
            else
                kit.UnsetFaceLights();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitGenericEdgesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitGenericEdgesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowGenericEdges(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetGenericEdges(_state);
            else
                kit.UnsetGenericEdges();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitInteriorSilhouetteEdgesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitInteriorSilhouetteEdgesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowInteriorSilhouetteEdges(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetInteriorSilhouetteEdges(_state);
            else
                kit.UnsetInteriorSilhouetteEdges();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitAdjacentEdgesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitAdjacentEdgesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowAdjacentEdges(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetAdjacentEdges(_state);
            else
                kit.UnsetAdjacentEdges();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitHardEdgesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitHardEdgesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowHardEdges(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetHardEdges(_state);
            else
                kit.UnsetHardEdges();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitMeshQuadEdgesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitMeshQuadEdgesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowMeshQuadEdges(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMeshQuadEdges(_state);
            else
                kit.UnsetMeshQuadEdges();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitNonCulledEdgesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitNonCulledEdgesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowNonCulledEdges(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetNonCulledEdges(_state);
            else
                kit.UnsetNonCulledEdges();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitPerimeterEdgesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitPerimeterEdgesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowPerimeterEdges(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetPerimeterEdges(_state);
            else
                kit.UnsetPerimeterEdges();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitFacesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitFacesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowFaces(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetFaces(_state);
            else
                kit.UnsetFaces();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitVerticesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitVerticesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowVertices(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVertices(_state);
            else
                kit.UnsetVertices();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitMarkersProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitMarkersProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowMarkers(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMarkers(_state);
            else
                kit.UnsetMarkers();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitShadowCastingProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitShadowCastingProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowShadowCasting(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetShadowCasting(_state);
            else
                kit.UnsetShadowCasting();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitShadowReceivingProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitShadowReceivingProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowShadowReceiving(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetShadowReceiving(_state);
            else
                kit.UnsetShadowReceiving();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitShadowEmittingProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitShadowEmittingProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowShadowEmitting(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetShadowEmitting(_state);
            else
                kit.UnsetShadowEmitting();
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisibilityKitLeaderLinesProperty : NestedProperty
    {
        private HPS.VisibilityKit kit;
        private bool set;
        private bool _state;

        public VisibilityKitLeaderLinesProperty(
            NestedProperty owner,
            HPS.VisibilityKit kit)
            : base(owner)
        {
            this.kit = kit;
            set = this.kit.ShowLeaderLines(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "State":
                    {
                        _state = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetLeaderLines(_state);
            else
                kit.UnsetLeaderLines();
            OnChildChanged();
        }
    }

    [DisplayName("Visibility")]
    public class VisibilityKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.VisibilityKit kit;

        public VisibilityKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowVisibility(out kit);

            CuttingSections = new VisibilityKitCuttingSectionsProperty(null, kit);
            CutEdges = new VisibilityKitCutEdgesProperty(null, kit);
            CutFaces = new VisibilityKitCutFacesProperty(null, kit);
            Windows = new VisibilityKitWindowsProperty(null, kit);
            Text = new VisibilityKitTextProperty(null, kit);
            Lines = new VisibilityKitLinesProperty(null, kit);
            EdgeLights = new VisibilityKitEdgeLightsProperty(null, kit);
            MarkerLights = new VisibilityKitMarkerLightsProperty(null, kit);
            FaceLights = new VisibilityKitFaceLightsProperty(null, kit);
            GenericEdges = new VisibilityKitGenericEdgesProperty(null, kit);
            InteriorSilhouetteEdges = new VisibilityKitInteriorSilhouetteEdgesProperty(null, kit);
            AdjacentEdges = new VisibilityKitAdjacentEdgesProperty(null, kit);
            HardEdges = new VisibilityKitHardEdgesProperty(null, kit);
            MeshQuadEdges = new VisibilityKitMeshQuadEdgesProperty(null, kit);
            NonCulledEdges = new VisibilityKitNonCulledEdgesProperty(null, kit);
            PerimeterEdges = new VisibilityKitPerimeterEdgesProperty(null, kit);
            Faces = new VisibilityKitFacesProperty(null, kit);
            Vertices = new VisibilityKitVerticesProperty(null, kit);
            Markers = new VisibilityKitMarkersProperty(null, kit);
            ShadowCasting = new VisibilityKitShadowCastingProperty(null, kit);
            ShadowReceiving = new VisibilityKitShadowReceivingProperty(null, kit);
            ShadowEmitting = new VisibilityKitShadowEmittingProperty(null, kit);
            LeaderLines = new VisibilityKitLeaderLinesProperty(null, kit);
        }

        [PropertyOrder(0)]
        public VisibilityKitCuttingSectionsProperty CuttingSections { get; set; }

        [PropertyOrder(1)]
        public VisibilityKitCutEdgesProperty CutEdges { get; set; }

        [PropertyOrder(2)]
        public VisibilityKitCutFacesProperty CutFaces { get; set; }

        [PropertyOrder(3)]
        public VisibilityKitWindowsProperty Windows { get; set; }

        [PropertyOrder(4)]
        public VisibilityKitTextProperty Text { get; set; }

        [PropertyOrder(5)]
        public VisibilityKitLinesProperty Lines { get; set; }

        [PropertyOrder(6)]
        public VisibilityKitEdgeLightsProperty EdgeLights { get; set; }

        [PropertyOrder(7)]
        public VisibilityKitMarkerLightsProperty MarkerLights { get; set; }

        [PropertyOrder(8)]
        public VisibilityKitFaceLightsProperty FaceLights { get; set; }

        [PropertyOrder(9)]
        public VisibilityKitGenericEdgesProperty GenericEdges { get; set; }

        [PropertyOrder(10)]
        public VisibilityKitInteriorSilhouetteEdgesProperty InteriorSilhouetteEdges { get; set; }

        [PropertyOrder(11)]
        public VisibilityKitAdjacentEdgesProperty AdjacentEdges { get; set; }

        [PropertyOrder(12)]
        public VisibilityKitHardEdgesProperty HardEdges { get; set; }

        [PropertyOrder(13)]
        public VisibilityKitMeshQuadEdgesProperty MeshQuadEdges { get; set; }

        [PropertyOrder(14)]
        public VisibilityKitNonCulledEdgesProperty NonCulledEdges { get; set; }

        [PropertyOrder(15)]
        public VisibilityKitPerimeterEdgesProperty PerimeterEdges { get; set; }

        [PropertyOrder(16)]
        public VisibilityKitFacesProperty Faces { get; set; }

        [PropertyOrder(17)]
        public VisibilityKitVerticesProperty Vertices { get; set; }

        [PropertyOrder(18)]
        public VisibilityKitMarkersProperty Markers { get; set; }

        [PropertyOrder(19)]
        public VisibilityKitShadowCastingProperty ShadowCasting { get; set; }

        [PropertyOrder(20)]
        public VisibilityKitShadowReceivingProperty ShadowReceiving { get; set; }

        [PropertyOrder(21)]
        public VisibilityKitShadowEmittingProperty ShadowEmitting { get; set; }

        [PropertyOrder(22)]
        public VisibilityKitLeaderLinesProperty LeaderLines { get; set; }

        public void Apply()
        {
            key.UnsetVisibility();
            key.SetVisibility(kit);
        }
    }
    #endregion

    #region CuttingSectionKit
    [ExpandableObject]
    public class CuttingSectionKitPlanesProperty : ArrayProperty
    {
        private HPS.CuttingSectionKit kit;
        private int count;
        private HPS.Plane[] planes;

        private ArrayList planeProperties;

        public CuttingSectionKitPlanesProperty(
            HPS.CuttingSectionKit kit)
            : base(null, "Plane")
        {
            this.kit = kit;
            this.kit.ShowPlanes(out planes);

            count = planes.Length;
            planeProperties = new ArrayList();

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref planes, newCount, HPS.Plane.Zero());
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                planeProperties.Add(new ArrayPlaneProperty(this, i, planes));
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(ArrayPlaneProperty), null, expandable: true));
            }
        }

        protected override void DeleteProperties(
            int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; --i)
            {
                Properties.RemoveAt(i + 1);
                planeProperties.RemoveAt(i);
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = planeProperties[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
            }
            OnChildChanged();
        }

        private void UpdateKit()
        {
            kit.SetPlanes(planes);
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    [ExpandableObject]
    public class CuttingSectionKitVisualizationProperty : NestedProperty
    {
        private HPS.CuttingSectionKit kit;
        private HPS.CuttingSection.Mode _mode;
        private HPS.RGBAColor _color;
        private RGBAColorProperty _colorProperty;
        private float _scale;

        public CuttingSectionKitVisualizationProperty(
            HPS.CuttingSectionKit kit)
            : base(null)
        {
            this.kit = kit;
            if (!this.kit.ShowVisualization(out _mode, out _color, out _scale))
            {
                _mode = HPS.CuttingSection.Mode.None;
                _color = new HPS.RGBAColor(0, 0, 0, 0.25f);
                _scale = 1.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Mode", typeof(HPS.CuttingSection.Mode), null, expandable: false, triggersRefresh: false));
            _colorProperty = new RGBAColorProperty(_color, Update_color);
            Properties.Add(new PropertySpec("Color", typeof(RGBAColorProperty), null, expandable: true, triggersRefresh: false));
            Properties.Add(new PropertySpec("Scale", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Mode": e.Value = _mode; break;
                case "Color": e.Value = _colorProperty; break;
                case "Scale": e.Value = _scale; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Mode":
                    {
                        _mode = (HPS.CuttingSection.Mode)e.Value;
                    }
                    break;
                case "Color":
                    {
                        // nothing to do
                    }
                    break;
                case "Scale":
                    {
                        _scale = (float)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetVisualization(_mode, _color, _scale);
        }

        private void Update_color(
            HPS.RGBAColor _color)
        {
            this._color = _color;
            UpdateKit();
        }
    }

    [DisplayName("CuttingSection")]
    public class CuttingSectionKitProperty : IRootProperty
    {
        private HPS.CuttingSectionKey key;
        private HPS.CuttingSectionKit kit;

        public CuttingSectionKitProperty(
            HPS.CuttingSectionKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Planes = new CuttingSectionKitPlanesProperty(kit);
            Visualization = new CuttingSectionKitVisualizationProperty(kit);
            Priority = new PriorityProperty<HPS.CuttingSectionKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.CuttingSectionKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public CuttingSectionKitPlanesProperty Planes { get; set; }

        [PropertyOrder(1)]
        public CuttingSectionKitVisualizationProperty Visualization { get; set; }

        [PropertyOrder(2)]
        public PriorityProperty<HPS.CuttingSectionKit> Priority { get; set; }

        [PropertyOrder(3)]
        public UserDataProperty<HPS.CuttingSectionKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region Custom VisualEffectsKit
    [ExpandableObject]
    public class VisualEffectsKitSimpleReflectionVisibilityProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private HPS.VisibilityKit _reflected_types;

        private VisibilityKitCuttingSectionsProperty cuttingSections;
        private VisibilityKitCutEdgesProperty cutEdges;
        private VisibilityKitCutFacesProperty cutFaces;
        private VisibilityKitWindowsProperty windows;
        private VisibilityKitTextProperty text;
        private VisibilityKitLinesProperty lines;
        private VisibilityKitEdgeLightsProperty edgeLights;
        private VisibilityKitMarkerLightsProperty markerLights;
        private VisibilityKitFaceLightsProperty faceLights;
        private VisibilityKitGenericEdgesProperty genericEdges;
        private VisibilityKitInteriorSilhouetteEdgesProperty interiorSilhouetteEdges;
        private VisibilityKitAdjacentEdgesProperty adjacentEdges;
        private VisibilityKitHardEdgesProperty hardEdges;
        private VisibilityKitMeshQuadEdgesProperty meshQuadEdges;
        private VisibilityKitNonCulledEdgesProperty nonCulledEdges;
        private VisibilityKitPerimeterEdgesProperty perimeterEdges;
        private VisibilityKitFacesProperty faces;
        private VisibilityKitVerticesProperty vertices;
        private VisibilityKitMarkersProperty markers;
        private VisibilityKitShadowCastingProperty shadowCasting;
        private VisibilityKitShadowReceivingProperty shadowReceiving;
        private VisibilityKitShadowEmittingProperty shadowEmitting;
        private VisibilityKitLeaderLinesProperty leaderLines;

        public VisualEffectsKitSimpleReflectionVisibilityProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSimpleReflectionVisibility(out _reflected_types);
            if (!set)
            {
                _reflected_types = new HPS.VisibilityKit();
            }

            cuttingSections = new VisibilityKitCuttingSectionsProperty(this, _reflected_types);
            cutEdges = new VisibilityKitCutEdgesProperty(this, _reflected_types);
            cutFaces = new VisibilityKitCutFacesProperty(this, _reflected_types);
            windows = new VisibilityKitWindowsProperty(this, _reflected_types);
            text = new VisibilityKitTextProperty(this, _reflected_types);
            lines = new VisibilityKitLinesProperty(this, _reflected_types);
            edgeLights = new VisibilityKitEdgeLightsProperty(this, _reflected_types);
            markerLights = new VisibilityKitMarkerLightsProperty(this, _reflected_types);
            faceLights = new VisibilityKitFaceLightsProperty(this, _reflected_types);
            genericEdges = new VisibilityKitGenericEdgesProperty(this, _reflected_types);
            interiorSilhouetteEdges = new VisibilityKitInteriorSilhouetteEdgesProperty(this, _reflected_types);
            adjacentEdges = new VisibilityKitAdjacentEdgesProperty(this, _reflected_types);
            hardEdges = new VisibilityKitHardEdgesProperty(this, _reflected_types);
            meshQuadEdges = new VisibilityKitMeshQuadEdgesProperty(this, _reflected_types);
            nonCulledEdges = new VisibilityKitNonCulledEdgesProperty(this, _reflected_types);
            perimeterEdges = new VisibilityKitPerimeterEdgesProperty(this, _reflected_types);
            faces = new VisibilityKitFacesProperty(this, _reflected_types);
            vertices = new VisibilityKitVerticesProperty(this, _reflected_types);
            markers = new VisibilityKitMarkersProperty(this, _reflected_types);
            shadowCasting = new VisibilityKitShadowCastingProperty(this, _reflected_types);
            shadowReceiving = new VisibilityKitShadowReceivingProperty(this, _reflected_types);
            shadowEmitting = new VisibilityKitShadowEmittingProperty(this, _reflected_types);
            leaderLines = new VisibilityKitLeaderLinesProperty(this, _reflected_types);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("CuttingSections", typeof(VisibilityKitCuttingSectionsProperty), null, expandable: true));
            Properties.Add(new PropertySpec("CutEdges", typeof(VisibilityKitCutEdgesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("CutFaces", typeof(VisibilityKitCutFacesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Windows", typeof(VisibilityKitWindowsProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Text", typeof(VisibilityKitTextProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Lines", typeof(VisibilityKitLinesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("EdgeLights", typeof(VisibilityKitEdgeLightsProperty), null, expandable: true));
            Properties.Add(new PropertySpec("MarkerLights", typeof(VisibilityKitMarkerLightsProperty), null, expandable: true));
            Properties.Add(new PropertySpec("FaceLights", typeof(VisibilityKitFaceLightsProperty), null, expandable: true));
            Properties.Add(new PropertySpec("GenericEdges", typeof(VisibilityKitGenericEdgesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("InteriorSilhouetteEdges", typeof(VisibilityKitInteriorSilhouetteEdgesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("AdjacentEdges", typeof(VisibilityKitAdjacentEdgesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("HardEdges", typeof(VisibilityKitHardEdgesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("MeshQuadEdges", typeof(VisibilityKitMeshQuadEdgesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("NonCulledEdges", typeof(VisibilityKitNonCulledEdgesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("PerimeterEdges", typeof(VisibilityKitPerimeterEdgesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Faces", typeof(VisibilityKitFacesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Vertices", typeof(VisibilityKitVerticesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Markers", typeof(VisibilityKitMarkersProperty), null, expandable: true));
            Properties.Add(new PropertySpec("ShadowCasting", typeof(VisibilityKitShadowCastingProperty), null, expandable: true));
            Properties.Add(new PropertySpec("ShadowReceiving", typeof(VisibilityKitShadowReceivingProperty), null, expandable: true));
            Properties.Add(new PropertySpec("ShadowEmitting", typeof(VisibilityKitShadowEmittingProperty), null, expandable: true));
            Properties.Add(new PropertySpec("LeaderLines", typeof(VisibilityKitLeaderLinesProperty), null, expandable: true));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "CuttingSections": e.Value = cuttingSections; break;
                case "CutEdges": e.Value = cutEdges; break;
                case "CutFaces": e.Value = cutFaces; break;
                case "Windows": e.Value = windows; break;
                case "Text": e.Value = text; break;
                case "Lines": e.Value = lines; break;
                case "EdgeLights": e.Value = edgeLights; break;
                case "MarkerLights": e.Value = markerLights; break;
                case "FaceLights": e.Value = faceLights; break;
                case "GenericEdges": e.Value = genericEdges; break;
                case "InteriorSilhouetteEdges": e.Value = interiorSilhouetteEdges; break;
                case "AdjacentEdges": e.Value = adjacentEdges; break;
                case "HardEdges": e.Value = hardEdges; break;
                case "MeshQuadEdges": e.Value = meshQuadEdges; break;
                case "NonCulledEdges": e.Value = nonCulledEdges; break;
                case "PerimeterEdges": e.Value = perimeterEdges; break;
                case "Faces": e.Value = faces; break;
                case "Vertices": e.Value = vertices; break;
                case "Markers": e.Value = markers; break;
                case "ShadowCasting": e.Value = shadowCasting; break;
                case "ShadowReceiving": e.Value = shadowReceiving; break;
                case "ShadowEmitting": e.Value = shadowEmitting; break;
                case "LeaderLines": e.Value = leaderLines; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                        UpdateKit();
                    }
                    break;
            }
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSimpleReflectionVisibility(_reflected_types);
            else
                kit.UnsetSimpleReflectionVisibility();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitEyeDomeLightingBackColorProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;
        private HPS.RGBColor _color;
        private RGBColorProperty _colorProperty;

        public VisualEffectsKitEyeDomeLightingBackColorProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowEyeDomeLightingBackColor(out _state, out _color);
            if (!set)
            {
                _state = true;
                _color = HPS.RGBColor.Black();
            }
            else if (!_state)
                _color = HPS.RGBColor.Black();

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            _colorProperty = new RGBColorProperty(_color, Update_color);
            Properties.Add(new PropertySpec("Color", typeof(RGBColorProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Color": e.Value = _colorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Color":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetEyeDomeLightingBackColor(_state, _color);
            else
                kit.UnsetEyeDomeLightingBackColor();
        }

        private void Update_color(
            HPS.RGBColor _color)
        {
            this._color = _color;
            UpdateKit();
        }
    }
    #endregion

    #region Custom DistantLightKit
    public class DistantLightKitColorProperty : SimpleMaterialProperty<HPS.DistantLightKit>
    {
        public DistantLightKitColorProperty(
            HPS.DistantLightKit kit)
            : base(kit.ShowColor, kit.SetColor, kit.SetColorByIndex, kit.UnsetColor)
        { }
    }
    #endregion

    #region Custom SpotlightKit
    public class SpotlightKitColorProperty : SimpleMaterialProperty<HPS.SpotlightKit>
    {
        public SpotlightKitColorProperty(
            HPS.SpotlightKit kit)
            : base(kit.ShowColor, kit.SetColor, kit.SetColorByIndex, kit.UnsetColor)
        { }
    }
    #endregion

    #region Custom TextKit
    public class TextKitColorProperty : SimpleMaterialProperty<HPS.TextKit>
    {
        public TextKitColorProperty(
            HPS.TextKit kit)
            : base(kit.ShowColor, kit.SetColor, kit.SetColorByIndex, kit.UnsetColor)
        { }
    }

    public class TextKitModellingMatrixProperty : KitMatrixProperty<HPS.TextKit>
    {
        public TextKitModellingMatrixProperty(
            HPS.TextKit kit)
            : base(kit.ShowModellingMatrix, kit.SetModellingMatrix, kit.UnsetModellingMatrix)
        { }
    }

    [ExpandableObject]
    public class RotationProperty<T> : NestedProperty
    {
        public delegate bool ShowRotation(out HPS.Text.Rotation rotation, out float angle);
        public delegate T SetRotation(HPS.Text.Rotation rotation, float angle);
        public delegate T UnsetRotation();

        private SetRotation setRotation;
        private UnsetRotation unsetRotation;
        private bool set;
        private HPS.Text.Rotation rotation;
        private float angle;

        public RotationProperty(
            ShowRotation showRotation,
            SetRotation setRotation,
            UnsetRotation unsetRotation)
            : base(null)
        {
            this.setRotation = setRotation;
            this.unsetRotation = unsetRotation;

            set = showRotation(out rotation, out angle);
            if (!set)
            {
                rotation = HPS.Text.Rotation.FollowPath;
                angle = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Rotation", typeof(HPS.Text.Rotation), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Angle", typeof(float), null, expandable: false));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Rotation": e.Value = rotation; break;
                case "Angle": e.Value = angle; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Rotation":
                    {
                        rotation = (HPS.Text.Rotation)e.Value;
                        EnableValidProperties();
                    }
                    break;
                case "Angle":
                    {
                        angle = (float)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Set = 0,
            Rotation = 1,
            Angle = 2
        }

        private void EnableValidProperties()
        {
            if (rotation == HPS.Text.Rotation.None || rotation == HPS.Text.Rotation.FollowPath)
                Properties[(int)PropertyTypeIndex.Angle].Enabled = false;
            else if (rotation == HPS.Text.Rotation.Rotate)
                Properties[(int)PropertyTypeIndex.Angle].Enabled = true;
        }

        private void UpdateKit()
        {
            if (set)
                setRotation(rotation, angle);
            else
                unsetRotation();
        }
    }

    public class TextKitRotationProperty : RotationProperty<HPS.TextKit>
    {
        public TextKitRotationProperty(
            HPS.TextKit kit)
            : base(kit.ShowRotation, kit.SetRotation, kit.UnsetRotation)
        { }
    }

    public enum RegionPointCount
    {
        Two = 2,
        Three = 3
    }

    [ExpandableObject]
    public class TextKitRegionProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private RegionPointCount _region_point_count;
        private HPS.Point[] _region;
        private HPS.Text.RegionAlignment _region_alignment;
        private HPS.Text.RegionFitting _region_fitting;
        private bool _region_adjust_direction;
        private bool _region_relative_coordinates;
        private bool _region_window_space;

        private PointProperty _region_point_0;
        private PointProperty _region_point_1;
        private PointProperty _region_point_2;

        public TextKitRegionProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowRegion(out _region, out _region_alignment, out _region_fitting, out _region_adjust_direction, out _region_relative_coordinates, out _region_window_space);
            if (set)
            {
                _region_point_count = (RegionPointCount)_region.Length;
                if (_region_point_count == RegionPointCount.Two)
                {
                    Array.Resize(ref _region, 3);
                    _region[2] = new HPS.Point(0, 0, 0);
                }
            }
            else
            {
                _region_point_count = RegionPointCount.Three;
                _region = new HPS.Point[] { new HPS.Point(0, 0, 0), new HPS.Point(1, 0, 0), new HPS.Point(0, 1, 0) };
                _region_alignment = HPS.Text.RegionAlignment.Bottom;
                _region_fitting = HPS.Text.RegionFitting.Left;
                _region_adjust_direction = true;
                _region_relative_coordinates = true;
                _region_window_space = false;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Region Point Count", typeof(RegionPointCount), null, expandable: false, triggersRefresh: true));
            _region_point_0 = new PointProperty(_region[0], UpdatePoint0);
            Properties.Add(new PropertySpec("Region Point 0", typeof(PointProperty), null, expandable: true));
            _region_point_1 = new PointProperty(_region[1], UpdatePoint1);
            Properties.Add(new PropertySpec("Region Point 1", typeof(PointProperty), null, expandable: true));
            _region_point_2 = new PointProperty(_region[2], UpdatePoint2);
            Properties.Add(new PropertySpec("Region Point 2", typeof(PointProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Region Alignment", typeof(HPS.Text.RegionAlignment), null, expandable: false));
            Properties.Add(new PropertySpec("Region Fitting", typeof(HPS.Text.RegionFitting), null, expandable: false));
            Properties.Add(new PropertySpec("Region Adjust Direction", typeof(bool), null, expandable: false));
            Properties.Add(new PropertySpec("Region Relative Coordinates", typeof(bool), null, expandable: false));
            Properties.Add(new PropertySpec("Region Window Space", typeof(bool), null, expandable: false));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Region Point Count": e.Value = _region_point_count; break;
                case "Region Point 0": e.Value = _region_point_0; break;
                case "Region Point 1": e.Value = _region_point_1; break;
                case "Region Point 2": e.Value = _region_point_2; break;
                case "Region Alignment": e.Value = _region_alignment; break;
                case "Region Fitting": e.Value = _region_fitting; break;
                case "Region Adjust Direction": e.Value = _region_adjust_direction; break;
                case "Region Relative Coordinates": e.Value = _region_relative_coordinates; break;
                case "Region Window Space": e.Value = _region_window_space; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Region Point Count":
                    {
                        _region_point_count = (RegionPointCount)e.Value;
                        EnableValidProperties();
                    }
                    break;
                case "Region Point 0":
                case "Region Point 1":
                case "Region Point 2":
                    {
                        // nothing to do
                    }
                    break;
                case "Region Alignment":
                    {
                        _region_alignment = (HPS.Text.RegionAlignment)e.Value;
                    }
                    break;
                case "Region Fitting":
                    {
                        _region_fitting = (HPS.Text.RegionFitting)e.Value;
                    }
                    break;
                case "Region Adjust Direction":
                    {
                        _region_adjust_direction = (bool)e.Value;
                    }
                    break;
                case "Region Relative Coordinates":
                    {
                        _region_relative_coordinates = (bool)e.Value;
                    }
                    break;
                case "Region Window Space":
                    {
                        _region_window_space = (bool)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Set = 0,
            Count = 1,
            Point0 = 2,
            Point1 = 3,
            Point2 = 4
        }

        private void EnableValidProperties()
        {
            if (_region_point_count == RegionPointCount.Two)
                Properties[(int)PropertyTypeIndex.Point2].Enabled = false;
            else if (_region_point_count == RegionPointCount.Three)
                Properties[(int)PropertyTypeIndex.Point2].Enabled = true;
        }

        private void UpdatePoint0(
            HPS.Point point)
        {
            _region[0] = point;
            UpdateKit();
        }

        private void UpdatePoint1(
            HPS.Point point)
        {
            _region[1] = point;
            UpdateKit();
        }

        private void UpdatePoint2(
            HPS.Point point)
        {
            _region[2] = point;
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetRegion(_region, _region_alignment, _region_fitting, _region_adjust_direction, _region_relative_coordinates, _region_window_space);
            else
                kit.UnsetRegion();
        }
    }

    [ExpandableObject]
    public class BackgroundMarginsArrayProperty : ArrayProperty
    {
        private int count;
        private float[] sizes;
        private HPS.Text.MarginUnits[] units;

        private ArrayList sizeProperties;

        public BackgroundMarginsArrayProperty(
            NestedProperty owner,
            float[] sizes,
            HPS.Text.MarginUnits[] units)
            : base(owner, "Margin")
        {
            this.sizes = sizes;
            this.units = units;

            count = this.sizes.Length;
            sizeProperties = new ArrayList();

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref sizes, newCount, 0.0f);
            PropertyUtilities.Resize(ref units, newCount, HPS.Text.MarginUnits.Percent);
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                sizeProperties.Add(new ArrayFloatUnitProperty<HPS.Text.MarginUnits>(this, "Size", "MarginUnits", i, sizes, units));
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(ArrayFloatUnitProperty<HPS.Text.MarginUnits>), null, expandable: true));
            }
        }

        protected override void DeleteProperties(
            int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; --i)
            {
                Properties.RemoveAt(i + 1);
                sizeProperties.RemoveAt(i);
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = sizeProperties[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
            }
            OnChildChanged();
        }

        public float[] Sizes
        {
            get { return sizes; }
        }

        public HPS.Text.MarginUnits[] Units
        {
            get { return units; }
        }
    }

    [ExpandableObject]
    public class BackgroundMarginsProperty<T> : NestedProperty
    {
        public delegate bool ShowBackgroundMargins(out float[] sizes, out HPS.Text.MarginUnits[] units);
        public delegate T SetBackgroundMargins(float[] sizes, HPS.Text.MarginUnits[] units);
        public delegate T UnsetBackgroundMargins();

        private SetBackgroundMargins setBackgroundMargins;
        private UnsetBackgroundMargins unsetBackgroundMargins;
        private bool set;

        private BackgroundMarginsArrayProperty marginsProperty;

        public BackgroundMarginsProperty(
            ShowBackgroundMargins showBackgroundMargins,
            SetBackgroundMargins setBackgroundMargins,
            UnsetBackgroundMargins unsetBackgroundMargins)
            : base(null)
        {
            this.setBackgroundMargins = setBackgroundMargins;
            this.unsetBackgroundMargins = unsetBackgroundMargins;
            float[] sizes;
            HPS.Text.MarginUnits[] units;
            set = showBackgroundMargins(out sizes, out units);
            if (!set)
            {
                sizes = new float[] { 0.0f };
                units = new HPS.Text.MarginUnits[] { HPS.Text.MarginUnits.Percent };
            }

            marginsProperty = new BackgroundMarginsArrayProperty(this, sizes, units);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Margins", typeof(BackgroundMarginsArrayProperty), null, expandable: true));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Margins": e.Value = marginsProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                setBackgroundMargins(marginsProperty.Sizes, marginsProperty.Units);
            else
                unsetBackgroundMargins();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    public class TextKitBackgroundMarginsProperty : BackgroundMarginsProperty<HPS.TextKit>
    {
        public TextKitBackgroundMarginsProperty(
            HPS.TextKit kit)
            : base(kit.ShowBackgroundMargins, kit.SetBackgroundMargins, kit.UnsetBackgroundMargins)
        { }
    }

    [ExpandableObject]
    public class TextKitLeaderLinesArrayProperty : ArrayProperty
    {
        private int count;
        private HPS.Point[] positions;

        private ArrayList positionProperties;

        public TextKitLeaderLinesArrayProperty(
            NestedProperty owner,
            HPS.Point[] positions)
            : base(owner, "Position")
        {
            this.positions = positions;

            count = this.positions.Length;
            positionProperties = new ArrayList();

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref positions, newCount, HPS.Point.Origin());
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                positionProperties.Add(new ArrayPointProperty(this, i, positions));
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(ArrayPointProperty), null, expandable: true));
            }
        }

        protected override void DeleteProperties(
            int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; --i)
            {
                Properties.RemoveAt(i + 1);
                positionProperties.RemoveAt(i);
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = positionProperties[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
            }
            OnChildChanged();
        }

        public HPS.Point[] Positions
        {
            get { return positions; }
        }
    }

    [ExpandableObject]
    public class TextKitLeaderLinesProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private HPS.Text.LeaderLineSpace space;

        private TextKitLeaderLinesArrayProperty positionsProperty;

        public TextKitLeaderLinesProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            HPS.Point[] positions;
            set = this.kit.ShowLeaderLines(out positions, out space);
            if (!set)
            {
                positions = new HPS.Point[] { HPS.Point.Origin() };
                space = HPS.Text.LeaderLineSpace.Object;
            }

            positionsProperty = new TextKitLeaderLinesArrayProperty(this, positions);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Space", typeof(HPS.Text.LeaderLineSpace), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Positions", typeof(TextKitLeaderLinesArrayProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Space": e.Value = space; break;
                case "Positions": e.Value = positionsProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Space":
                    {
                        space = (HPS.Text.LeaderLineSpace)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetLeaderLines(positionsProperty.Positions, space);
            else
                kit.UnsetLeaderLines();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }
    #endregion

    #region Custom TextAttributeKit
    public class TextAttributeKitRotationProperty : RotationProperty<HPS.TextAttributeKit>
    {
        public TextAttributeKitRotationProperty(
            HPS.TextAttributeKit kit)
            : base(kit.ShowRotation, kit.SetRotation, kit.UnsetRotation)
        { }
    }

    public class TextAttributeKitBackgroundMarginsProperty : BackgroundMarginsProperty<HPS.TextAttributeKit>
    {
        public TextAttributeKitBackgroundMarginsProperty(
            HPS.TextAttributeKit kit)
            : base(kit.ShowBackgroundMargins, kit.SetBackgroundMargins, kit.UnsetBackgroundMargins)
        { }
    }
    #endregion

    #region Custom BoundingKit
    public enum BoundingVolumeType
    {
        Sphere,
        Cuboid
    }

    public class BoundingKitVolumeProperty : NestedProperty
    {
        private HPS.BoundingKit kit;
        private bool set;
        private BoundingVolumeType type;
        private HPS.SimpleSphere sphere;
        private HPS.SimpleCuboid cuboid;

        private SimpleSphereProperty sphereProperty;
        private SimpleCuboidProperty cuboidProperty;

        public BoundingKitVolumeProperty(
            HPS.BoundingKit kit)
            : base(null)
        {
            this.kit = kit;

            set = this.kit.ShowVolume(out sphere, out cuboid);
            if (!set)
            {
                sphere = new HPS.SimpleSphere(HPS.Point.Origin(), 1);
                cuboid = new HPS.SimpleCuboid(new HPS.Point(-1, -1, -1), new HPS.Point(1, 1, 1));
            }
            type = BoundingVolumeType.Cuboid;

            sphereProperty = new SimpleSphereProperty(sphere, UpdateSphere);
            cuboidProperty = new SimpleCuboidProperty(cuboid, UpdateCuboid);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Type", typeof(BoundingVolumeType), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Sphere", typeof(HPS.Text.Rotation), null));
            Properties.Add(new PropertySpec("Cuboid", typeof(float), null));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Type": e.Value = type; break;
                case "Sphere": e.Value = sphereProperty; break;
                case "Cuboid": e.Value = cuboidProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Type":
                    {
                        type = (BoundingVolumeType)e.Value;
                        EnableValidProperties();
                    }
                    break;
                case "Sphere":
                case "Cuboid":
                    {
                        // nothing to do
                    }
                    break;
            }
            UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Set = 0,
            Type = 1,
            Sphere = 2,
            Cuboid = 3
        }

        private void EnableValidProperties()
        {
            if (type == BoundingVolumeType.Sphere)
            {
                Properties[(int)PropertyTypeIndex.Sphere].Enabled = true;
                Properties[(int)PropertyTypeIndex.Cuboid].Enabled = false;
            }
            else if (type == BoundingVolumeType.Cuboid)
            {
                Properties[(int)PropertyTypeIndex.Sphere].Enabled = false;
                Properties[(int)PropertyTypeIndex.Cuboid].Enabled = true;
            }
        }

        private void UpdateKit()
        {
            if (set)
            {
                if (type == BoundingVolumeType.Sphere)
                    kit.SetVolume(sphere);
                else if (type == BoundingVolumeType.Cuboid)
                    kit.SetVolume(cuboid);
            }
            else
                kit.UnsetVolume();
        }

        private void UpdateSphere(
            HPS.SimpleSphere sphere)
        {
            this.sphere = sphere;
            UpdateKit();
        }

        private void UpdateCuboid(
            HPS.SimpleCuboid cuboid)
        {
            this.cuboid = cuboid;
            UpdateKit();
        }
    }
    #endregion

    #region Custom ContourLineKit
    [ExpandableObject]
    public class ContourLineKitPositionsArrayProperty : ArrayProperty
    {
        private int count;
        private float[] positions;

        public ContourLineKitPositionsArrayProperty(
            NestedProperty owner,
            float[] positions)
            : base(owner, "Position")
        {
            this.positions = positions;

            count = this.positions.Length;
            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref positions, newCount, 0.0f);
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(float), null, expandable: false));
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = positions[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
                default:
                    {
                        positions[GetIndexFromName(e)] = (float)e.Value;
                    }
                    break;
            }
            OnChildChanged();
        }

        public float[] Positions
        {
            get { return positions; }
        }
    }

    [ExpandableObject]
    public class ContourLineKitPositionsProperty : NestedProperty
    {
        private HPS.ContourLineKit kit;
        private bool set;
        private HPS.ContourLine.Mode mode;
        private float interval;
        private float offset;

        private ContourLineKitPositionsArrayProperty positionsProperty;

        public ContourLineKitPositionsProperty(
            HPS.ContourLineKit kit)
            : base(null)
        {
            this.kit = kit;
            float[] positions;
            set = this.kit.ShowPositions(out mode, out positions);
            if (set)
            {
                if (mode == HPS.ContourLine.Mode.Repeating)
                {
                    interval = positions[0];
                    offset = positions[1];
                    positions = new float[] { 0.0f };
                }
                else if (mode == HPS.ContourLine.Mode.Explicit)
                {
                    interval = 1.0f;
                    offset = 0.0f;
                }
            }
            else
            {
                mode = HPS.ContourLine.Mode.Repeating;
                interval = 1.0f;
                offset = 0.0f;
                positions = new float[] { 0.0f };
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Mode", typeof(HPS.ContourLine.Mode), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Interval", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Offset", typeof(float), null, expandable: false, triggersRefresh: false));
            positionsProperty = new ContourLineKitPositionsArrayProperty(this, positions);
            Properties.Add(new PropertySpec("Positions", typeof(ContourLineKitPositionsArrayProperty), null, expandable: true, triggersRefresh: false));
            EnableValidProperties();
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Mode": e.Value = mode; break;
                case "Interval": e.Value = interval; break;
                case "Offset": e.Value = offset; break;
                case "Positions": e.Value = positionsProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Mode":
                    {
                        mode = (HPS.ContourLine.Mode)e.Value;
                        EnableValidProperties();
                    }
                    break;
                case "Interval":
                    {
                        interval = (float)e.Value;
                    }
                    break;
                case "Offset":
                    {
                        offset = (float)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private enum PropertyTypeIndex
        {
            Set = 0,
            Mode = 1,
            Interval = 2,
            Offset = 3,
            Positions = 4
        }

        private void EnableValidProperties()
        {
            if (mode == HPS.ContourLine.Mode.Repeating)
            {
                Properties[(int)PropertyTypeIndex.Interval].Enabled = true;
                Properties[(int)PropertyTypeIndex.Offset].Enabled = true;
                Properties[(int)PropertyTypeIndex.Positions].Enabled = false;
            }
            else if (mode == HPS.ContourLine.Mode.Explicit)
            {
                Properties[(int)PropertyTypeIndex.Interval].Enabled = false;
                Properties[(int)PropertyTypeIndex.Offset].Enabled = false;
                Properties[(int)PropertyTypeIndex.Positions].Enabled = true;
            }
        }

        private void UpdateKit()
        {
            if (set)
            {
                if (mode == HPS.ContourLine.Mode.Repeating)
                    kit.SetPositions(interval, offset);
                else if (mode == HPS.ContourLine.Mode.Explicit)
                    kit.SetPositions(positionsProperty.Positions);
            }
            else
                kit.UnsetPositions();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    [ExpandableObject]
    public class ContourLineKitColorsArrayProperty : ArrayProperty
    {
        private int count;
        private HPS.RGBColor[] colors;

        private ArrayList colorProperties;

        public ContourLineKitColorsArrayProperty(
            NestedProperty owner,
            HPS.RGBColor[] colors)
            : base(owner, "Color")
        {
            this.colors = colors;

            count = this.colors.Length;
            colorProperties = new ArrayList();

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref colors, newCount, HPS.RGBColor.Black());
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                colorProperties.Add(new ArrayColorProperty(this, i, colors));
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(ArrayColorProperty), null, expandable: true));
            }
        }

        protected override void DeleteProperties(
            int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; --i)
            {
                Properties.RemoveAt(i + 1);
                colorProperties.RemoveAt(i);
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = colorProperties[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
            }
            OnChildChanged();
        }

        public HPS.RGBColor[] Colors
        {
            get { return colors; }
        }
    }

    [ExpandableObject]
    public class ContourLineKitColorsProperty : NestedProperty
    {
        private HPS.ContourLineKit kit;
        private bool set;

        private ContourLineKitColorsArrayProperty colorsProperty;

        public ContourLineKitColorsProperty(
            HPS.ContourLineKit kit)
            : base(null)
        {
            this.kit = kit;
            HPS.RGBColor[] colors;
            set = this.kit.ShowColors(out colors);
            if (!set)
                colors = new HPS.RGBColor[] { HPS.RGBColor.Black() };

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            colorsProperty = new ContourLineKitColorsArrayProperty(this, colors);
            Properties.Add(new PropertySpec("Colors", typeof(ContourLineKitColorsArrayProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Colors": e.Value = colorsProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetColors(colorsProperty.Colors);
            else
                kit.UnsetColors();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    [ExpandableObject]
    public class ContourLineKitPatternsArrayProperty : ArrayProperty
    {
        private int count;
        private string[] patterns;

        public ContourLineKitPatternsArrayProperty(
            NestedProperty owner,
            string[] patterns)
            : base(owner, "Pattern")
        {
            this.patterns = patterns;

            count = this.patterns.Length;

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref patterns, newCount, "");
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(string), null, expandable: false));
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = patterns[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
                default:
                    {
                        patterns[GetIndexFromName(e)] = (string)e.Value;
                    }
                    break;
            }
            OnChildChanged();
        }

        public string[] Patterns
        {
            get { return patterns; }
        }
    }

    [ExpandableObject]
    public class ContourLineKitPatternsProperty : NestedProperty
    {
        private HPS.ContourLineKit kit;
        private bool set;

        private ContourLineKitPatternsArrayProperty patternsProperty;

        public ContourLineKitPatternsProperty(
            HPS.ContourLineKit kit)
            : base(null)
        {
            this.kit = kit;
            string[] patterns;
            set = this.kit.ShowPatterns(out patterns);
            if (!set)
                patterns = new string[] { "" };

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            patternsProperty = new ContourLineKitPatternsArrayProperty(this, patterns);
            Properties.Add(new PropertySpec("Patterns", typeof(ContourLineKitPatternsArrayProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Patterns": e.Value = patternsProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetPatterns(patternsProperty.Patterns);
            else
                kit.UnsetPatterns();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    [ExpandableObject]
    public class ContourLineKitWeightsArrayProperty : ArrayProperty
    {
        private int count;
        private float[] weights;
        private HPS.Line.SizeUnits[] units;

        private ArrayList weightProperties;

        public ContourLineKitWeightsArrayProperty(
            NestedProperty owner,
            float[] weights,
            HPS.Line.SizeUnits[] units)
            : base(owner, "Weight")
        {
            this.weights = weights;
            this.units = units;

            count = this.weights.Length;
            weightProperties = new ArrayList();

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref weights, newCount, 1.0f);
            PropertyUtilities.Resize(ref units, newCount, HPS.Line.SizeUnits.ScaleFactor);
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                weightProperties.Add(new ArrayFloatUnitProperty<HPS.Line.SizeUnits>(this, itemPrefix, "SizeUnits", i, weights, units));
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(ArrayFloatUnitProperty<HPS.Line.SizeUnits>), null, expandable: true));
            }
        }

        protected override void DeleteProperties(
            int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; --i)
            {
                Properties.RemoveAt(i + 1);
                weightProperties.RemoveAt(i);
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = weightProperties[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
            }
            OnChildChanged();
        }

        public float[] Weights
        {
            get { return weights; }
        }

        public HPS.Line.SizeUnits[] Units
        {
            get { return units; }
        }
    }

    [ExpandableObject]
    public class ContourLineKitWeightsProperty : NestedProperty
    {
        private HPS.ContourLineKit kit;
        private bool set;

        private ContourLineKitWeightsArrayProperty weightsProperty;

        public ContourLineKitWeightsProperty(
            HPS.ContourLineKit kit)
            : base(null)
        {
            this.kit = kit;
            float[] weights;
            HPS.Line.SizeUnits[] units;
            set = this.kit.ShowWeights(out weights, out units);
            if (!set)
            {
                weights = new float[] { 1.0f };
                units = new HPS.Line.SizeUnits[] { HPS.Line.SizeUnits.ScaleFactor };
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            weightsProperty = new ContourLineKitWeightsArrayProperty(this, weights, units);
            Properties.Add(new PropertySpec("Weights", typeof(ContourLineKitWeightsArrayProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Weights": e.Value = weightsProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetWeights(weightsProperty.Weights, weightsProperty.Units);
            else
                kit.UnsetWeights();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }
    #endregion

    #region Custom DrawingAttributeKit
    [ExpandableObject]
    public class SingleClipRegionLoopProperty : ArrayProperty
    {
        private int index;
        private HPS.Point[][] loops;

        private int count;
        private ArrayList pointProperties;

        public SingleClipRegionLoopProperty(
            NestedProperty owner,
            int index,
            HPS.Point[][] loops)
            : base(owner, "Point")
        {
            this.index = index;
            this.loops = loops;

            count = this.loops[index].Length;
            pointProperties = new ArrayList();

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            PropertyUtilities.Resize(ref loops[index], newCount, new HPS.Point(0, 0, 0));
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                pointProperties.Add(new ArrayPointProperty(this, i, loops[index]));
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(ArrayPointProperty), null, expandable: true));
            }
        }

        protected override void DeleteProperties(
            int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; --i)
            {
                Properties.RemoveAt(i + 1);
                pointProperties.RemoveAt(i);
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = pointProperties[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
            }
            OnChildChanged();
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitClipRegionLoopsArrayProperty : ArrayProperty
    {
        private int count;
        private HPS.Point[][] loops;

        private ArrayList loopProperties;

        public DrawingAttributeKitClipRegionLoopsArrayProperty(
            NestedProperty owner,
            HPS.Point[][] loops)
            : base(owner, "Loop")
        {
            this.loops = loops;

            count = this.loops.Length;
            loopProperties = new ArrayList();

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Count", typeof(int), null, expandable: false, triggersRefresh: true));
            AddProperties(count);
        }

        protected override void ResizeArrays(
            int newCount)
        {
            int oldCount = loops.Length;
            Array.Resize(ref loops, newCount);
            for (int i = oldCount; i < newCount; ++i)
                loops[i] = new HPS.Point[] { HPS.Point.Origin() };
        }

        protected override void AddProperties(
            int newCount)
        {
            for (int i = 0; i < newCount; ++i)
            {
                loopProperties.Add(new SingleClipRegionLoopProperty(this, i, loops));
                string ithName = itemPrefix + " " + i.ToString();
                Properties.Add(new PropertySpec(ithName, typeof(SingleClipRegionLoopProperty), null, expandable: true));
            }
        }

        protected override void DeleteProperties(
            int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; --i)
            {
                Properties.RemoveAt(i + 1);
                loopProperties.RemoveAt(i);
            }
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count": e.Value = count; break;
                default:
                    {
                        e.Value = loopProperties[GetIndexFromName(e)];
                    }
                    break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Count":
                    {
                        int newCount = PropertyUtilities.Clamp((int)e.Value, 1, int.MaxValue);
                        if (AddOrDeleteItems(count, newCount))
                            count = newCount;
                    }
                    break;
            }
            OnChildChanged();
        }

        public HPS.Point[][] Loops
        {
            get { return loops; }
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitClipRegionProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private HPS.Drawing.ClipSpace space;
        private HPS.Drawing.ClipOperation operation;

        private DrawingAttributeKitClipRegionLoopsArrayProperty loopsProperty;

        public DrawingAttributeKitClipRegionProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            HPS.Point[][] loops;
            set = this.kit.ShowClipRegion(out loops, out space, out operation);
            if (!set)
            {
                loops = new HPS.Point[][] { new HPS.Point[] { HPS.Point.Origin() } };
                space = HPS.Drawing.ClipSpace.World;
                operation = HPS.Drawing.ClipOperation.Keep;
            }

            loopsProperty = new DrawingAttributeKitClipRegionLoopsArrayProperty(this, loops);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Space", typeof(HPS.Drawing.ClipSpace), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Operation", typeof(HPS.Drawing.ClipOperation), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Loops", typeof(DrawingAttributeKitClipRegionLoopsArrayProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Space": e.Value = space; break;
                case "Operation": e.Value = operation; break;
                case "Loops": e.Value = loopsProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                    }
                    break;
                case "Space":
                    {
                        space = (HPS.Drawing.ClipSpace)e.Value;
                    }
                    break;
                case "Operation":
                    {
                        operation = (HPS.Drawing.ClipOperation)e.Value;
                    }
                    break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetClipRegion(loopsProperty.Loops, space, operation);
            else
                kit.UnsetClipRegion();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitOverrideInternalColorProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private HPS.VisibilityKit _kit;

        private VisibilityKitTextProperty text;
        private VisibilityKitLinesProperty lines;
        private VisibilityKitGenericEdgesProperty genericEdges;
        private VisibilityKitFacesProperty faces;
        private VisibilityKitVerticesProperty vertices;
        private VisibilityKitMarkersProperty markers;

        public DrawingAttributeKitOverrideInternalColorProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowOverrideInternalColor(out _kit);
            if (!set)
            {
                _kit = new HPS.VisibilityKit();
                _kit.SetEverything(false);
            }

            text = new VisibilityKitTextProperty(this, _kit);
            lines = new VisibilityKitLinesProperty(this, _kit);
            genericEdges = new VisibilityKitGenericEdgesProperty(this, _kit);
            faces = new VisibilityKitFacesProperty(this, _kit);
            vertices = new VisibilityKitVerticesProperty(this, _kit);
            markers = new VisibilityKitMarkersProperty(this, _kit);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Text", typeof(VisibilityKitTextProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Lines", typeof(VisibilityKitLinesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("GenericEdges", typeof(VisibilityKitGenericEdgesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Faces", typeof(VisibilityKitFacesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Vertices", typeof(VisibilityKitVerticesProperty), null, expandable: true));
            Properties.Add(new PropertySpec("Markers", typeof(VisibilityKitMarkersProperty), null, expandable: true));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Text": e.Value = text; break;
                case "Lines": e.Value = lines; break;
                case "GenericEdges": e.Value = genericEdges; break;
                case "Faces": e.Value = faces; break;
                case "Vertices": e.Value = vertices; break;
                case "Markers": e.Value = markers; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                    {
                        set = (bool)e.Value;
                        PropertyUtilities.SetBrowsable(Properties, set);
                        UpdateKit();
                    }
                    break;
            }
        }

        private void UpdateKit()
        {
            if (set)
                kit.ShowOverrideInternalColor(out _kit);
            else
                kit.UnsetOverrideInternalColor();
        }

        protected override void OnChildChanged()
        {
            UpdateKit();
            base.OnChildChanged();
        }
    }


    #endregion

    #region TextKit
    [ExpandableObject]
    public class TextKitPositionProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private HPS.Point _position;
        private PointProperty _positionProperty;

        public TextKitPositionProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowPosition(out _position);

            GetValue += Get;
            SetValue += Set;
            _positionProperty = new PointProperty(_position, Update_position);
            Properties.Add(new PropertySpec("Position", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Position": e.Value = _positionProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Position":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetPosition(_position);
        }

        private void Update_position(
            HPS.Point _position)
        {
            this._position = _position;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class TextKitTextProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private string _string;

        public TextKitTextProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowText(out _string);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("String", typeof(string), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "String": e.Value = _string; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "String":
                {
                    _string = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetText(_string);
        }
    }

    [ExpandableObject]
    public class TextKitAlignmentProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private HPS.Text.Alignment _alignment;
        private HPS.Text.ReferenceFrame _reference_frame;
        private HPS.Text.Justification _justification;

        public TextKitAlignmentProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowAlignment(out _alignment, out _reference_frame, out _justification);
            if (!set)
            {
                _alignment = HPS.Text.Alignment.BottomLeft;
                _reference_frame = HPS.Text.ReferenceFrame.WorldAligned;
                _justification = HPS.Text.Justification.Left;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Alignment", typeof(HPS.Text.Alignment), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Reference Frame", typeof(HPS.Text.ReferenceFrame), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Justification", typeof(HPS.Text.Justification), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Alignment": e.Value = _alignment; break;
                case "Reference Frame": e.Value = _reference_frame; break;
                case "Justification": e.Value = _justification; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Alignment":
                {
                    _alignment = (HPS.Text.Alignment)e.Value;
                }
                break;
                case "Reference Frame":
                {
                    _reference_frame = (HPS.Text.ReferenceFrame)e.Value;
                }
                break;
                case "Justification":
                {
                    _justification = (HPS.Text.Justification)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetAlignment(_alignment, _reference_frame, _justification);
            else
                kit.UnsetAlignment();
        }
    }

    [ExpandableObject]
    public class TextKitBoldProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private bool _state;

        public TextKitBoldProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBold(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBold(_state);
            else
                kit.UnsetBold();
        }
    }

    [ExpandableObject]
    public class TextKitItalicProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private bool _state;

        public TextKitItalicProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowItalic(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetItalic(_state);
            else
                kit.UnsetItalic();
        }
    }

    [ExpandableObject]
    public class TextKitOverlineProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private bool _state;

        public TextKitOverlineProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowOverline(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetOverline(_state);
            else
                kit.UnsetOverline();
        }
    }

    [ExpandableObject]
    public class TextKitStrikethroughProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private bool _state;

        public TextKitStrikethroughProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowStrikethrough(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetStrikethrough(_state);
            else
                kit.UnsetStrikethrough();
        }
    }

    [ExpandableObject]
    public class TextKitUnderlineProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private bool _state;

        public TextKitUnderlineProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowUnderline(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetUnderline(_state);
            else
                kit.UnsetUnderline();
        }
    }

    [ExpandableObject]
    public class TextKitSlantProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private float _angle;

        public TextKitSlantProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSlant(out _angle);
            if (!set)
            {
                _angle = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Angle", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Angle": e.Value = _angle; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Angle":
                {
                    _angle = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSlant(_angle);
            else
                kit.UnsetSlant();
        }
    }

    [ExpandableObject]
    public class TextKitLineSpacingProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private float _multiplier;

        public TextKitLineSpacingProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowLineSpacing(out _multiplier);
            if (!set)
            {
                _multiplier = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Multiplier", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Multiplier": e.Value = _multiplier; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Multiplier":
                {
                    _multiplier = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetLineSpacing(_multiplier);
            else
                kit.UnsetLineSpacing();
        }
    }

    [ExpandableObject]
    public class TextKitExtraSpaceProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private bool _state;
        private float _size;
        private HPS.Text.SizeUnits _units;

        public TextKitExtraSpaceProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowExtraSpace(out _state, out _size, out _units);
            if (!set)
            {
                _state = true;
                _size = 0.0f;
                _units = HPS.Text.SizeUnits.Points;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Size", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Text.SizeUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Size": e.Value = _size; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Size":
                {
                    _size = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Text.SizeUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetExtraSpace(_state, _size, _units);
            else
                kit.UnsetExtraSpace();
        }
    }

    [ExpandableObject]
    public class TextKitGreekingProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private bool _state;
        private float _size;
        private HPS.Text.GreekingUnits _units;
        private HPS.Text.GreekingMode _mode;

        public TextKitGreekingProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowGreeking(out _state, out _size, out _units, out _mode);
            if (!set)
            {
                _state = true;
                _size = 0.0f;
                _units = HPS.Text.GreekingUnits.Pixels;
                _mode = HPS.Text.GreekingMode.Lines;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Size", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Text.GreekingUnits), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Mode", typeof(HPS.Text.GreekingMode), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Size": e.Value = _size; break;
                case "Units": e.Value = _units; break;
                case "Mode": e.Value = _mode; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Size":
                {
                    _size = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Text.GreekingUnits)e.Value;
                }
                break;
                case "Mode":
                {
                    _mode = (HPS.Text.GreekingMode)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetGreeking(_state, _size, _units, _mode);
            else
                kit.UnsetGreeking();
        }
    }

    [ExpandableObject]
    public class TextKitSizeToleranceProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private bool _state;
        private float _size;
        private HPS.Text.SizeToleranceUnits _units;

        public TextKitSizeToleranceProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSizeTolerance(out _state, out _size, out _units);
            if (!set)
            {
                _state = true;
                _size = 0.0f;
                _units = HPS.Text.SizeToleranceUnits.Percent;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Size", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Text.SizeToleranceUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Size": e.Value = _size; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Size":
                {
                    _size = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Text.SizeToleranceUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSizeTolerance(_state, _size, _units);
            else
                kit.UnsetSizeTolerance();
        }
    }

    [ExpandableObject]
    public class TextKitSizeProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private float _size;
        private HPS.Text.SizeUnits _units;

        public TextKitSizeProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSize(out _size, out _units);
            if (!set)
            {
                _size = 0.0f;
                _units = HPS.Text.SizeUnits.Points;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Size", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Text.SizeUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Size": e.Value = _size; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Size":
                {
                    _size = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Text.SizeUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSize(_size, _units);
            else
                kit.UnsetSize();
        }
    }

    [ExpandableObject]
    public class TextKitFontProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private string _name;

        public TextKitFontProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowFont(out _name);
            if (!set)
            {
                _name = "name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Name", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Name": e.Value = _name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Name":
                {
                    _name = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetFont(_name);
            else
                kit.UnsetFont();
        }
    }

    [ExpandableObject]
    public class TextKitTransformProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private HPS.Text.Transform _trans;

        public TextKitTransformProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowTransform(out _trans);
            if (!set)
            {
                _trans = HPS.Text.Transform.Transformable;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Trans", typeof(HPS.Text.Transform), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Trans": e.Value = _trans; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Trans":
                {
                    _trans = (HPS.Text.Transform)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetTransform(_trans);
            else
                kit.UnsetTransform();
        }
    }

    [ExpandableObject]
    public class TextKitRendererProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private HPS.Text.Renderer _renderer;

        public TextKitRendererProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowRenderer(out _renderer);
            if (!set)
            {
                _renderer = HPS.Text.Renderer.Default;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Renderer", typeof(HPS.Text.Renderer), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Renderer": e.Value = _renderer; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Renderer":
                {
                    _renderer = (HPS.Text.Renderer)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetRenderer(_renderer);
            else
                kit.UnsetRenderer();
        }
    }

    [ExpandableObject]
    public class TextKitPreferenceProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private float _cutoff;
        private HPS.Text.SizeUnits _units;
        private HPS.Text.Preference _smaller;
        private HPS.Text.Preference _larger;

        public TextKitPreferenceProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowPreference(out _cutoff, out _units, out _smaller, out _larger);
            if (!set)
            {
                _cutoff = 0.0f;
                _units = HPS.Text.SizeUnits.Points;
                _smaller = HPS.Text.Preference.Default;
                _larger = HPS.Text.Preference.Default;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Cutoff", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Text.SizeUnits), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Smaller", typeof(HPS.Text.Preference), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Larger", typeof(HPS.Text.Preference), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Cutoff": e.Value = _cutoff; break;
                case "Units": e.Value = _units; break;
                case "Smaller": e.Value = _smaller; break;
                case "Larger": e.Value = _larger; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Cutoff":
                {
                    _cutoff = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Text.SizeUnits)e.Value;
                }
                break;
                case "Smaller":
                {
                    _smaller = (HPS.Text.Preference)e.Value;
                }
                break;
                case "Larger":
                {
                    _larger = (HPS.Text.Preference)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetPreference(_cutoff, _units, _smaller, _larger);
            else
                kit.UnsetPreference();
        }
    }

    [ExpandableObject]
    public class TextKitPathProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private HPS.Vector _path;
        private VectorProperty _pathProperty;

        public TextKitPathProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowPath(out _path);
            if (!set)
            {
                _path = HPS.Vector.Unit();
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            _pathProperty = new VectorProperty(_path, Update_path);
            Properties.Add(new PropertySpec("Path", typeof(VectorProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Path": e.Value = _pathProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Path":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetPath(_path);
            else
                kit.UnsetPath();
        }

        private void Update_path(
            HPS.Vector _path)
        {
            this._path = _path;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class TextKitSpacingProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private float _multiplier;

        public TextKitSpacingProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSpacing(out _multiplier);
            if (!set)
            {
                _multiplier = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Multiplier", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Multiplier": e.Value = _multiplier; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Multiplier":
                {
                    _multiplier = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSpacing(_multiplier);
            else
                kit.UnsetSpacing();
        }
    }

    [ExpandableObject]
    public class TextKitBackgroundProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private bool _state;
        private string _name;

        public TextKitBackgroundProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBackground(out _state, out _name);
            if (!set)
            {
                _state = true;
                _name = "name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Name", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Name": e.Value = _name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Name":
                {
                    _name = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBackground(_state, _name);
            else
                kit.UnsetBackground();
        }
    }

    [ExpandableObject]
    public class TextKitBackgroundStyleProperty : NestedProperty
    {
        private HPS.TextKit kit;
        private bool set;
        private string _name;

        public TextKitBackgroundStyleProperty(
            HPS.TextKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBackgroundStyle(out _name);
            if (!set)
            {
                _name = "name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Name", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Name": e.Value = _name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Name":
                {
                    _name = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBackgroundStyle(_name);
            else
                kit.UnsetBackgroundStyle();
        }
    }

    [DisplayName("Text")]
    public class TextKitProperty : IRootProperty
    {
        private HPS.TextKey key;
        private HPS.TextKit kit;

        public TextKitProperty(
            HPS.TextKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Position = new TextKitPositionProperty(kit);
            Text = new TextKitTextProperty(kit);
            Color = new TextKitColorProperty(kit);
            ModellingMatrix = new TextKitModellingMatrixProperty(kit);
            Alignment = new TextKitAlignmentProperty(kit);
            Bold = new TextKitBoldProperty(kit);
            Italic = new TextKitItalicProperty(kit);
            Overline = new TextKitOverlineProperty(kit);
            Strikethrough = new TextKitStrikethroughProperty(kit);
            Underline = new TextKitUnderlineProperty(kit);
            Slant = new TextKitSlantProperty(kit);
            LineSpacing = new TextKitLineSpacingProperty(kit);
            Rotation = new TextKitRotationProperty(kit);
            ExtraSpace = new TextKitExtraSpaceProperty(kit);
            Greeking = new TextKitGreekingProperty(kit);
            SizeTolerance = new TextKitSizeToleranceProperty(kit);
            Size = new TextKitSizeProperty(kit);
            Font = new TextKitFontProperty(kit);
            Transform = new TextKitTransformProperty(kit);
            Renderer = new TextKitRendererProperty(kit);
            Preference = new TextKitPreferenceProperty(kit);
            Path = new TextKitPathProperty(kit);
            Spacing = new TextKitSpacingProperty(kit);
            Background = new TextKitBackgroundProperty(kit);
            BackgroundMargins = new TextKitBackgroundMarginsProperty(kit);
            BackgroundStyle = new TextKitBackgroundStyleProperty(kit);
            LeaderLines = new TextKitLeaderLinesProperty(kit);
            Region = new TextKitRegionProperty(kit);
            Priority = new PriorityProperty<HPS.TextKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.TextKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public TextKitPositionProperty Position { get; set; }

        [PropertyOrder(1)]
        public TextKitTextProperty Text { get; set; }

        [PropertyOrder(2)]
        public TextKitColorProperty Color { get; set; }

        [PropertyOrder(3)]
        public TextKitModellingMatrixProperty ModellingMatrix { get; set; }

        [PropertyOrder(4)]
        public TextKitAlignmentProperty Alignment { get; set; }

        [PropertyOrder(5)]
        public TextKitBoldProperty Bold { get; set; }

        [PropertyOrder(6)]
        public TextKitItalicProperty Italic { get; set; }

        [PropertyOrder(7)]
        public TextKitOverlineProperty Overline { get; set; }

        [PropertyOrder(8)]
        public TextKitStrikethroughProperty Strikethrough { get; set; }

        [PropertyOrder(9)]
        public TextKitUnderlineProperty Underline { get; set; }

        [PropertyOrder(10)]
        public TextKitSlantProperty Slant { get; set; }

        [PropertyOrder(11)]
        public TextKitLineSpacingProperty LineSpacing { get; set; }

        [PropertyOrder(12)]
        public TextKitRotationProperty Rotation { get; set; }

        [PropertyOrder(13)]
        public TextKitExtraSpaceProperty ExtraSpace { get; set; }

        [PropertyOrder(14)]
        public TextKitGreekingProperty Greeking { get; set; }

        [PropertyOrder(15)]
        public TextKitSizeToleranceProperty SizeTolerance { get; set; }

        [PropertyOrder(16)]
        public TextKitSizeProperty Size { get; set; }

        [PropertyOrder(17)]
        public TextKitFontProperty Font { get; set; }

        [PropertyOrder(18)]
        public TextKitTransformProperty Transform { get; set; }

        [PropertyOrder(19)]
        public TextKitRendererProperty Renderer { get; set; }

        [PropertyOrder(20)]
        public TextKitPreferenceProperty Preference { get; set; }

        [PropertyOrder(21)]
        public TextKitPathProperty Path { get; set; }

        [PropertyOrder(22)]
        public TextKitSpacingProperty Spacing { get; set; }

        [PropertyOrder(23)]
        public TextKitBackgroundProperty Background { get; set; }

        [PropertyOrder(24)]
        public TextKitBackgroundMarginsProperty BackgroundMargins { get; set; }

        [PropertyOrder(25)]
        public TextKitBackgroundStyleProperty BackgroundStyle { get; set; }

        [PropertyOrder(26)]
        public TextKitLeaderLinesProperty LeaderLines { get; set; }

        [PropertyOrder(27)]
        public TextKitRegionProperty Region { get; set; }

        [PropertyOrder(28)]
        public PriorityProperty<HPS.TextKit> Priority { get; set; }

        [PropertyOrder(29)]
        public UserDataProperty<HPS.TextKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region TransparencyKit
    [ExpandableObject]
    public class TransparencyKitMethodProperty : NestedProperty
    {
        private HPS.TransparencyKit kit;
        private bool set;
        private HPS.Transparency.Method _style;

        public TransparencyKitMethodProperty(
            HPS.TransparencyKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMethod(out _style);
            if (!set)
            {
                _style = HPS.Transparency.Method.Blended;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Style", typeof(HPS.Transparency.Method), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Style": e.Value = _style; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Style":
                {
                    _style = (HPS.Transparency.Method)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMethod(_style);
            else
                kit.UnsetMethod();
        }
    }

    [ExpandableObject]
    public class TransparencyKitAlgorithmProperty : NestedProperty
    {
        private HPS.TransparencyKit kit;
        private bool set;
        private HPS.Transparency.Algorithm _algorithm;

        public TransparencyKitAlgorithmProperty(
            HPS.TransparencyKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowAlgorithm(out _algorithm);
            if (!set)
            {
                _algorithm = HPS.Transparency.Algorithm.DepthPeeling;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Algorithm", typeof(HPS.Transparency.Algorithm), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Algorithm": e.Value = _algorithm; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Algorithm":
                {
                    _algorithm = (HPS.Transparency.Algorithm)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetAlgorithm(_algorithm);
            else
                kit.UnsetAlgorithm();
        }
    }

    [ExpandableObject]
    public class TransparencyKitDepthPeelingLayersProperty : NestedProperty
    {
        private HPS.TransparencyKit kit;
        private bool set;
        private uint _layers;

        public TransparencyKitDepthPeelingLayersProperty(
            HPS.TransparencyKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDepthPeelingLayers(out _layers);
            if (!set)
            {
                _layers = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Layers", typeof(uint), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Layers": e.Value = _layers; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Layers":
                {
                    _layers = (uint)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDepthPeelingLayers(_layers);
            else
                kit.UnsetDepthPeelingLayers();
        }
    }

    [ExpandableObject]
    public class TransparencyKitDepthPeelingMinimumAreaProperty : NestedProperty
    {
        private HPS.TransparencyKit kit;
        private bool set;
        private float _area;
        private HPS.Transparency.AreaUnits _units;

        public TransparencyKitDepthPeelingMinimumAreaProperty(
            HPS.TransparencyKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDepthPeelingMinimumArea(out _area, out _units);
            if (!set)
            {
                _area = 0.0f;
                _units = HPS.Transparency.AreaUnits.Pixels;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Area", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Transparency.AreaUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Area": e.Value = _area; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Area":
                {
                    _area = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Transparency.AreaUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDepthPeelingMinimumArea(_area, _units);
            else
                kit.UnsetDepthPeelingMinimumArea();
        }
    }

    [ExpandableObject]
    public class TransparencyKitDepthWritingProperty : NestedProperty
    {
        private HPS.TransparencyKit kit;
        private bool set;
        private bool _state;

        public TransparencyKitDepthWritingProperty(
            HPS.TransparencyKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDepthWriting(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDepthWriting(_state);
            else
                kit.UnsetDepthWriting();
        }
    }

    [ExpandableObject]
    public class TransparencyKitDepthPeelingPreferenceProperty : NestedProperty
    {
        private HPS.TransparencyKit kit;
        private bool set;
        private HPS.Transparency.Preference _preference;

        public TransparencyKitDepthPeelingPreferenceProperty(
            HPS.TransparencyKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDepthPeelingPreference(out _preference);
            if (!set)
            {
                _preference = HPS.Transparency.Preference.Fastest;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Preference", typeof(HPS.Transparency.Preference), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Preference": e.Value = _preference; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Preference":
                {
                    _preference = (HPS.Transparency.Preference)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDepthPeelingPreference(_preference);
            else
                kit.UnsetDepthPeelingPreference();
        }
    }

    [DisplayName("Transparency")]
    public class TransparencyKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.TransparencyKit kit;

        public TransparencyKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowTransparency(out kit);

            Method = new TransparencyKitMethodProperty(kit);
            Algorithm = new TransparencyKitAlgorithmProperty(kit);
            DepthPeelingLayers = new TransparencyKitDepthPeelingLayersProperty(kit);
            DepthPeelingMinimumArea = new TransparencyKitDepthPeelingMinimumAreaProperty(kit);
            DepthWriting = new TransparencyKitDepthWritingProperty(kit);
            DepthPeelingPreference = new TransparencyKitDepthPeelingPreferenceProperty(kit);
        }

        [PropertyOrder(0)]
        public TransparencyKitMethodProperty Method { get; set; }

        [PropertyOrder(1)]
        public TransparencyKitAlgorithmProperty Algorithm { get; set; }

        [PropertyOrder(2)]
        public TransparencyKitDepthPeelingLayersProperty DepthPeelingLayers { get; set; }

        [PropertyOrder(3)]
        public TransparencyKitDepthPeelingMinimumAreaProperty DepthPeelingMinimumArea { get; set; }

        [PropertyOrder(4)]
        public TransparencyKitDepthWritingProperty DepthWriting { get; set; }

        [PropertyOrder(5)]
        public TransparencyKitDepthPeelingPreferenceProperty DepthPeelingPreference { get; set; }

        public void Apply()
        {
            key.UnsetTransparency();
            key.SetTransparency(kit);
        }
    }
    #endregion

    #region CullingKit
    [ExpandableObject]
    public class CullingKitDeferralExtentProperty : NestedProperty
    {
        private HPS.CullingKit kit;
        private bool set;
        private bool _state;
        private uint _pixels;

        public CullingKitDeferralExtentProperty(
            HPS.CullingKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDeferralExtent(out _state, out _pixels);
            if (!set)
            {
                _state = true;
                _pixels = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Pixels", typeof(uint), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Pixels": e.Value = _pixels; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Pixels":
                {
                    _pixels = (uint)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDeferralExtent(_state, _pixels);
            else
                kit.UnsetDeferralExtent();
        }
    }

    [ExpandableObject]
    public class CullingKitExtentProperty : NestedProperty
    {
        private HPS.CullingKit kit;
        private bool set;
        private bool _state;
        private uint _pixels;

        public CullingKitExtentProperty(
            HPS.CullingKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowExtent(out _state, out _pixels);
            if (!set)
            {
                _state = true;
                _pixels = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Pixels", typeof(uint), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Pixels": e.Value = _pixels; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Pixels":
                {
                    _pixels = (uint)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetExtent(_state, _pixels);
            else
                kit.UnsetExtent();
        }
    }

    [ExpandableObject]
    public class CullingKitBackFaceProperty : NestedProperty
    {
        private HPS.CullingKit kit;
        private bool set;
        private bool _state;

        public CullingKitBackFaceProperty(
            HPS.CullingKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBackFace(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBackFace(_state);
            else
                kit.UnsetBackFace();
        }
    }

    [ExpandableObject]
    public class CullingKitFaceProperty : NestedProperty
    {
        private HPS.CullingKit kit;
        private bool set;
        private HPS.Culling.Face _state;

        public CullingKitFaceProperty(
            HPS.CullingKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowFace(out _state);
            if (!set)
            {
                _state = HPS.Culling.Face.Back;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(HPS.Culling.Face), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (HPS.Culling.Face)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetFace(_state);
            else
                kit.UnsetFace();
        }
    }

    [ExpandableObject]
    public class CullingKitVectorProperty : NestedProperty
    {
        private HPS.CullingKit kit;
        private bool set;
        private bool _state;
        private HPS.Vector _vector;
        private VectorProperty _vectorProperty;

        public CullingKitVectorProperty(
            HPS.CullingKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowVector(out _state, out _vector);
            if (!set)
            {
                _state = true;
                _vector = HPS.Vector.Unit();
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            _vectorProperty = new VectorProperty(_vector, Update_vector);
            Properties.Add(new PropertySpec("Vector", typeof(VectorProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Vector": e.Value = _vectorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Vector":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVector(_state, _vector);
            else
                kit.UnsetVector();
        }

        private void Update_vector(
            HPS.Vector _vector)
        {
            this._vector = _vector;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class CullingKitVectorToleranceProperty : NestedProperty
    {
        private HPS.CullingKit kit;
        private bool set;
        private float _tolerance_degrees;

        public CullingKitVectorToleranceProperty(
            HPS.CullingKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowVectorTolerance(out _tolerance_degrees);
            if (!set)
            {
                _tolerance_degrees = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Tolerance Degrees", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Tolerance Degrees": e.Value = _tolerance_degrees; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Tolerance Degrees":
                {
                    _tolerance_degrees = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVectorTolerance(_tolerance_degrees);
            else
                kit.UnsetVectorTolerance();
        }
    }

    [ExpandableObject]
    public class CullingKitFrustumProperty : NestedProperty
    {
        private HPS.CullingKit kit;
        private bool set;
        private bool _state;

        public CullingKitFrustumProperty(
            HPS.CullingKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowFrustum(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetFrustum(_state);
            else
                kit.UnsetFrustum();
        }
    }

    [ExpandableObject]
    public class CullingKitVolumeProperty : NestedProperty
    {
        private HPS.CullingKit kit;
        private bool set;
        private bool _state;
        private HPS.SimpleCuboid _volume;
        private SimpleCuboidProperty _volumeProperty;

        public CullingKitVolumeProperty(
            HPS.CullingKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowVolume(out _state, out _volume);
            if (!set)
            {
                _state = true;
                _volume = new HPS.SimpleCuboid(new HPS.Point(-1, -1, -1), new HPS.Point(1, 1, 1));
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            _volumeProperty = new SimpleCuboidProperty(_volume, Update_volume);
            Properties.Add(new PropertySpec("Volume", typeof(SimpleCuboidProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Volume": e.Value = _volumeProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
                case "Volume":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVolume(_state, _volume);
            else
                kit.UnsetVolume();
        }

        private void Update_volume(
            HPS.SimpleCuboid _volume)
        {
            this._volume = _volume;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class CullingKitDistanceProperty : NestedProperty
    {
        private HPS.CullingKit kit;
        private bool set;
        private bool _state;
        private float _max_distance;

        public CullingKitDistanceProperty(
            HPS.CullingKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDistance(out _state, out _max_distance);
            if (!set)
            {
                _state = true;
                _max_distance = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Max Distance", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Max Distance": e.Value = _max_distance; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
                case "Max Distance":
                {
                    _max_distance = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDistance(_state, _max_distance);
            else
                kit.UnsetDistance();
        }
    }

    [DisplayName("Culling")]
    public class CullingKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.CullingKit kit;

        public CullingKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowCulling(out kit);

            DeferralExtent = new CullingKitDeferralExtentProperty(kit);
            Extent = new CullingKitExtentProperty(kit);
            BackFace = new CullingKitBackFaceProperty(kit);
            Face = new CullingKitFaceProperty(kit);
            Vector = new CullingKitVectorProperty(kit);
            VectorTolerance = new CullingKitVectorToleranceProperty(kit);
            Frustum = new CullingKitFrustumProperty(kit);
            Volume = new CullingKitVolumeProperty(kit);
            Distance = new CullingKitDistanceProperty(kit);
        }

        [PropertyOrder(0)]
        public CullingKitDeferralExtentProperty DeferralExtent { get; set; }

        [PropertyOrder(1)]
        public CullingKitExtentProperty Extent { get; set; }

        [PropertyOrder(2)]
        public CullingKitBackFaceProperty BackFace { get; set; }

        [PropertyOrder(3)]
        public CullingKitFaceProperty Face { get; set; }

        [PropertyOrder(4)]
        public CullingKitVectorProperty Vector { get; set; }

        [PropertyOrder(5)]
        public CullingKitVectorToleranceProperty VectorTolerance { get; set; }

        [PropertyOrder(6)]
        public CullingKitFrustumProperty Frustum { get; set; }

        [PropertyOrder(7)]
        public CullingKitVolumeProperty Volume { get; set; }

        [PropertyOrder(8)]
        public CullingKitDistanceProperty Distance { get; set; }

        public void Apply()
        {
            key.UnsetCulling();
            key.SetCulling(kit);
        }
    }
    #endregion

    #region TextAttributeKit
    [ExpandableObject]
    public class TextAttributeKitAlignmentProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private HPS.Text.Alignment _align;
        private HPS.Text.ReferenceFrame _ref;
        private HPS.Text.Justification _justify;

        public TextAttributeKitAlignmentProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowAlignment(out _align, out _ref, out _justify);
            if (!set)
            {
                _align = HPS.Text.Alignment.BottomLeft;
                _ref = HPS.Text.ReferenceFrame.WorldAligned;
                _justify = HPS.Text.Justification.Left;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Align", typeof(HPS.Text.Alignment), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Ref", typeof(HPS.Text.ReferenceFrame), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Justify", typeof(HPS.Text.Justification), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Align": e.Value = _align; break;
                case "Ref": e.Value = _ref; break;
                case "Justify": e.Value = _justify; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Align":
                {
                    _align = (HPS.Text.Alignment)e.Value;
                }
                break;
                case "Ref":
                {
                    _ref = (HPS.Text.ReferenceFrame)e.Value;
                }
                break;
                case "Justify":
                {
                    _justify = (HPS.Text.Justification)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetAlignment(_align, _ref, _justify);
            else
                kit.UnsetAlignment();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitBoldProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private bool _state;

        public TextAttributeKitBoldProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBold(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBold(_state);
            else
                kit.UnsetBold();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitItalicProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private bool _state;

        public TextAttributeKitItalicProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowItalic(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetItalic(_state);
            else
                kit.UnsetItalic();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitOverlineProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private bool _state;

        public TextAttributeKitOverlineProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowOverline(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetOverline(_state);
            else
                kit.UnsetOverline();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitStrikethroughProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private bool _state;

        public TextAttributeKitStrikethroughProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowStrikethrough(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetStrikethrough(_state);
            else
                kit.UnsetStrikethrough();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitUnderlineProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private bool _state;

        public TextAttributeKitUnderlineProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowUnderline(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetUnderline(_state);
            else
                kit.UnsetUnderline();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitSlantProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private float _angle;

        public TextAttributeKitSlantProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSlant(out _angle);
            if (!set)
            {
                _angle = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Angle", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Angle": e.Value = _angle; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Angle":
                {
                    _angle = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSlant(_angle);
            else
                kit.UnsetSlant();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitLineSpacingProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private float _multiplier;

        public TextAttributeKitLineSpacingProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowLineSpacing(out _multiplier);
            if (!set)
            {
                _multiplier = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Multiplier", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Multiplier": e.Value = _multiplier; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Multiplier":
                {
                    _multiplier = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetLineSpacing(_multiplier);
            else
                kit.UnsetLineSpacing();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitExtraSpaceProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private bool _state;
        private float _size;
        private HPS.Text.SizeUnits _units;

        public TextAttributeKitExtraSpaceProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowExtraSpace(out _state, out _size, out _units);
            if (!set)
            {
                _state = true;
                _size = 0.0f;
                _units = HPS.Text.SizeUnits.Points;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Size", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Text.SizeUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Size": e.Value = _size; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Size":
                {
                    _size = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Text.SizeUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetExtraSpace(_state, _size, _units);
            else
                kit.UnsetExtraSpace();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitGreekingProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private bool _state;
        private float _size;
        private HPS.Text.GreekingUnits _units;
        private HPS.Text.GreekingMode _mode;

        public TextAttributeKitGreekingProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowGreeking(out _state, out _size, out _units, out _mode);
            if (!set)
            {
                _state = true;
                _size = 0.0f;
                _units = HPS.Text.GreekingUnits.Pixels;
                _mode = HPS.Text.GreekingMode.Lines;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Size", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Text.GreekingUnits), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Mode", typeof(HPS.Text.GreekingMode), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Size": e.Value = _size; break;
                case "Units": e.Value = _units; break;
                case "Mode": e.Value = _mode; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Size":
                {
                    _size = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Text.GreekingUnits)e.Value;
                }
                break;
                case "Mode":
                {
                    _mode = (HPS.Text.GreekingMode)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetGreeking(_state, _size, _units, _mode);
            else
                kit.UnsetGreeking();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitSizeToleranceProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private bool _state;
        private float _size;
        private HPS.Text.SizeToleranceUnits _units;

        public TextAttributeKitSizeToleranceProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSizeTolerance(out _state, out _size, out _units);
            if (!set)
            {
                _state = true;
                _size = 0.0f;
                _units = HPS.Text.SizeToleranceUnits.Percent;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Size", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Text.SizeToleranceUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Size": e.Value = _size; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Size":
                {
                    _size = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Text.SizeToleranceUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSizeTolerance(_state, _size, _units);
            else
                kit.UnsetSizeTolerance();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitSizeProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private float _size;
        private HPS.Text.SizeUnits _units;

        public TextAttributeKitSizeProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSize(out _size, out _units);
            if (!set)
            {
                _size = 0.0f;
                _units = HPS.Text.SizeUnits.Points;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Size", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Text.SizeUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Size": e.Value = _size; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Size":
                {
                    _size = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Text.SizeUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSize(_size, _units);
            else
                kit.UnsetSize();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitFontProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private string _name;

        public TextAttributeKitFontProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowFont(out _name);
            if (!set)
            {
                _name = "name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Name", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Name": e.Value = _name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Name":
                {
                    _name = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetFont(_name);
            else
                kit.UnsetFont();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitTransformProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private HPS.Text.Transform _trans;

        public TextAttributeKitTransformProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowTransform(out _trans);
            if (!set)
            {
                _trans = HPS.Text.Transform.Transformable;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Trans", typeof(HPS.Text.Transform), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Trans": e.Value = _trans; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Trans":
                {
                    _trans = (HPS.Text.Transform)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetTransform(_trans);
            else
                kit.UnsetTransform();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitRendererProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private HPS.Text.Renderer _rend;

        public TextAttributeKitRendererProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowRenderer(out _rend);
            if (!set)
            {
                _rend = HPS.Text.Renderer.Default;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Rend", typeof(HPS.Text.Renderer), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Rend": e.Value = _rend; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Rend":
                {
                    _rend = (HPS.Text.Renderer)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetRenderer(_rend);
            else
                kit.UnsetRenderer();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitPreferenceProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private float _cutoff;
        private HPS.Text.SizeUnits _units;
        private HPS.Text.Preference _smaller;
        private HPS.Text.Preference _larger;

        public TextAttributeKitPreferenceProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowPreference(out _cutoff, out _units, out _smaller, out _larger);
            if (!set)
            {
                _cutoff = 0.0f;
                _units = HPS.Text.SizeUnits.Points;
                _smaller = HPS.Text.Preference.Default;
                _larger = HPS.Text.Preference.Default;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Cutoff", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Text.SizeUnits), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Smaller", typeof(HPS.Text.Preference), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Larger", typeof(HPS.Text.Preference), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Cutoff": e.Value = _cutoff; break;
                case "Units": e.Value = _units; break;
                case "Smaller": e.Value = _smaller; break;
                case "Larger": e.Value = _larger; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Cutoff":
                {
                    _cutoff = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Text.SizeUnits)e.Value;
                }
                break;
                case "Smaller":
                {
                    _smaller = (HPS.Text.Preference)e.Value;
                }
                break;
                case "Larger":
                {
                    _larger = (HPS.Text.Preference)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetPreference(_cutoff, _units, _smaller, _larger);
            else
                kit.UnsetPreference();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitPathProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private HPS.Vector _path;
        private VectorProperty _pathProperty;

        public TextAttributeKitPathProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowPath(out _path);
            if (!set)
            {
                _path = HPS.Vector.Unit();
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            _pathProperty = new VectorProperty(_path, Update_path);
            Properties.Add(new PropertySpec("Path", typeof(VectorProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Path": e.Value = _pathProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Path":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetPath(_path);
            else
                kit.UnsetPath();
        }

        private void Update_path(
            HPS.Vector _path)
        {
            this._path = _path;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitSpacingProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private float _multiplier;

        public TextAttributeKitSpacingProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSpacing(out _multiplier);
            if (!set)
            {
                _multiplier = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Multiplier", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Multiplier": e.Value = _multiplier; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Multiplier":
                {
                    _multiplier = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSpacing(_multiplier);
            else
                kit.UnsetSpacing();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitBackgroundProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private bool _state;
        private string _name;

        public TextAttributeKitBackgroundProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBackground(out _state, out _name);
            if (!set)
            {
                _state = true;
                _name = "name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Name", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Name": e.Value = _name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Name":
                {
                    _name = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBackground(_state, _name);
            else
                kit.UnsetBackground();
        }
    }

    [ExpandableObject]
    public class TextAttributeKitBackgroundStyleProperty : NestedProperty
    {
        private HPS.TextAttributeKit kit;
        private bool set;
        private string _name;

        public TextAttributeKitBackgroundStyleProperty(
            HPS.TextAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBackgroundStyle(out _name);
            if (!set)
            {
                _name = "name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Name", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Name": e.Value = _name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Name":
                {
                    _name = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBackgroundStyle(_name);
            else
                kit.UnsetBackgroundStyle();
        }
    }

    [DisplayName("TextAttribute")]
    public class TextAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.TextAttributeKit kit;

        public TextAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowTextAttribute(out kit);

            Alignment = new TextAttributeKitAlignmentProperty(kit);
            Bold = new TextAttributeKitBoldProperty(kit);
            Italic = new TextAttributeKitItalicProperty(kit);
            Overline = new TextAttributeKitOverlineProperty(kit);
            Strikethrough = new TextAttributeKitStrikethroughProperty(kit);
            Underline = new TextAttributeKitUnderlineProperty(kit);
            Slant = new TextAttributeKitSlantProperty(kit);
            LineSpacing = new TextAttributeKitLineSpacingProperty(kit);
            Rotation = new TextAttributeKitRotationProperty(kit);
            ExtraSpace = new TextAttributeKitExtraSpaceProperty(kit);
            Greeking = new TextAttributeKitGreekingProperty(kit);
            SizeTolerance = new TextAttributeKitSizeToleranceProperty(kit);
            Size = new TextAttributeKitSizeProperty(kit);
            Font = new TextAttributeKitFontProperty(kit);
            Transform = new TextAttributeKitTransformProperty(kit);
            Renderer = new TextAttributeKitRendererProperty(kit);
            Preference = new TextAttributeKitPreferenceProperty(kit);
            Path = new TextAttributeKitPathProperty(kit);
            Spacing = new TextAttributeKitSpacingProperty(kit);
            Background = new TextAttributeKitBackgroundProperty(kit);
            BackgroundMargins = new TextAttributeKitBackgroundMarginsProperty(kit);
            BackgroundStyle = new TextAttributeKitBackgroundStyleProperty(kit);
        }

        [PropertyOrder(0)]
        public TextAttributeKitAlignmentProperty Alignment { get; set; }

        [PropertyOrder(1)]
        public TextAttributeKitBoldProperty Bold { get; set; }

        [PropertyOrder(2)]
        public TextAttributeKitItalicProperty Italic { get; set; }

        [PropertyOrder(3)]
        public TextAttributeKitOverlineProperty Overline { get; set; }

        [PropertyOrder(4)]
        public TextAttributeKitStrikethroughProperty Strikethrough { get; set; }

        [PropertyOrder(5)]
        public TextAttributeKitUnderlineProperty Underline { get; set; }

        [PropertyOrder(6)]
        public TextAttributeKitSlantProperty Slant { get; set; }

        [PropertyOrder(7)]
        public TextAttributeKitLineSpacingProperty LineSpacing { get; set; }

        [PropertyOrder(8)]
        public TextAttributeKitRotationProperty Rotation { get; set; }

        [PropertyOrder(9)]
        public TextAttributeKitExtraSpaceProperty ExtraSpace { get; set; }

        [PropertyOrder(10)]
        public TextAttributeKitGreekingProperty Greeking { get; set; }

        [PropertyOrder(11)]
        public TextAttributeKitSizeToleranceProperty SizeTolerance { get; set; }

        [PropertyOrder(12)]
        public TextAttributeKitSizeProperty Size { get; set; }

        [PropertyOrder(13)]
        public TextAttributeKitFontProperty Font { get; set; }

        [PropertyOrder(14)]
        public TextAttributeKitTransformProperty Transform { get; set; }

        [PropertyOrder(15)]
        public TextAttributeKitRendererProperty Renderer { get; set; }

        [PropertyOrder(16)]
        public TextAttributeKitPreferenceProperty Preference { get; set; }

        [PropertyOrder(17)]
        public TextAttributeKitPathProperty Path { get; set; }

        [PropertyOrder(18)]
        public TextAttributeKitSpacingProperty Spacing { get; set; }

        [PropertyOrder(19)]
        public TextAttributeKitBackgroundProperty Background { get; set; }

        [PropertyOrder(20)]
        public TextAttributeKitBackgroundMarginsProperty BackgroundMargins { get; set; }

        [PropertyOrder(21)]
        public TextAttributeKitBackgroundStyleProperty BackgroundStyle { get; set; }

        public void Apply()
        {
            key.UnsetTextAttribute();
            key.SetTextAttribute(kit);
        }
    }
    #endregion

    #region EdgeAttributeKit
    [ExpandableObject]
    public class EdgeAttributeKitPatternProperty : NestedProperty
    {
        private HPS.EdgeAttributeKit kit;
        private bool set;
        private string _pattern_name;

        public EdgeAttributeKitPatternProperty(
            HPS.EdgeAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowPattern(out _pattern_name);
            if (!set)
            {
                _pattern_name = "pattern_name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Pattern Name", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Pattern Name": e.Value = _pattern_name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Pattern Name":
                {
                    _pattern_name = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetPattern(_pattern_name);
            else
                kit.UnsetPattern();
        }
    }

    [ExpandableObject]
    public class EdgeAttributeKitWeightProperty : NestedProperty
    {
        private HPS.EdgeAttributeKit kit;
        private bool set;
        private float _weight;
        private HPS.Edge.SizeUnits _units;

        public EdgeAttributeKitWeightProperty(
            HPS.EdgeAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowWeight(out _weight, out _units);
            if (!set)
            {
                _weight = 0.0f;
                _units = HPS.Edge.SizeUnits.ScaleFactor;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Weight", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Edge.SizeUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Weight": e.Value = _weight; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Weight":
                {
                    _weight = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Edge.SizeUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetWeight(_weight, _units);
            else
                kit.UnsetWeight();
        }
    }

    [ExpandableObject]
    public class EdgeAttributeKitHardAngleProperty : NestedProperty
    {
        private HPS.EdgeAttributeKit kit;
        private bool set;
        private float _angle;

        public EdgeAttributeKitHardAngleProperty(
            HPS.EdgeAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowHardAngle(out _angle);
            if (!set)
            {
                _angle = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Angle", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Angle": e.Value = _angle; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Angle":
                {
                    _angle = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetHardAngle(_angle);
            else
                kit.UnsetHardAngle();
        }
    }

    [DisplayName("EdgeAttribute")]
    public class EdgeAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.EdgeAttributeKit kit;

        public EdgeAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowEdgeAttribute(out kit);

            Pattern = new EdgeAttributeKitPatternProperty(kit);
            Weight = new EdgeAttributeKitWeightProperty(kit);
            HardAngle = new EdgeAttributeKitHardAngleProperty(kit);
        }

        [PropertyOrder(0)]
        public EdgeAttributeKitPatternProperty Pattern { get; set; }

        [PropertyOrder(1)]
        public EdgeAttributeKitWeightProperty Weight { get; set; }

        [PropertyOrder(2)]
        public EdgeAttributeKitHardAngleProperty HardAngle { get; set; }

        public void Apply()
        {
            key.UnsetEdgeAttribute();
            key.SetEdgeAttribute(kit);
        }
    }
    #endregion

    #region CurveAttributeKit
    [ExpandableObject]
    public class CurveAttributeKitBudgetProperty : NestedProperty
    {
        private HPS.CurveAttributeKit kit;
        private bool set;
        private ulong _budget;

        public CurveAttributeKitBudgetProperty(
            HPS.CurveAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBudget(out _budget);
            if (!set)
            {
                _budget = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Budget", typeof(ulong), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Budget": e.Value = _budget; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Budget":
                {
                    _budget = (ulong)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBudget(_budget);
            else
                kit.UnsetBudget();
        }
    }

    [ExpandableObject]
    public class CurveAttributeKitContinuedBudgetProperty : NestedProperty
    {
        private HPS.CurveAttributeKit kit;
        private bool set;
        private bool _state;
        private ulong _budget;

        public CurveAttributeKitContinuedBudgetProperty(
            HPS.CurveAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowContinuedBudget(out _state, out _budget);
            if (!set)
            {
                _state = true;
                _budget = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Budget", typeof(ulong), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Budget": e.Value = _budget; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Budget":
                {
                    _budget = (ulong)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetContinuedBudget(_state, _budget);
            else
                kit.UnsetContinuedBudget();
        }
    }

    [ExpandableObject]
    public class CurveAttributeKitViewDependentProperty : NestedProperty
    {
        private HPS.CurveAttributeKit kit;
        private bool set;
        private bool _state;

        public CurveAttributeKitViewDependentProperty(
            HPS.CurveAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowViewDependent(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetViewDependent(_state);
            else
                kit.UnsetViewDependent();
        }
    }

    [ExpandableObject]
    public class CurveAttributeKitMaximumDeviationProperty : NestedProperty
    {
        private HPS.CurveAttributeKit kit;
        private bool set;
        private float _deviation;

        public CurveAttributeKitMaximumDeviationProperty(
            HPS.CurveAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMaximumDeviation(out _deviation);
            if (!set)
            {
                _deviation = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Deviation", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Deviation": e.Value = _deviation; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Deviation":
                {
                    _deviation = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMaximumDeviation(_deviation);
            else
                kit.UnsetMaximumDeviation();
        }
    }

    [ExpandableObject]
    public class CurveAttributeKitMaximumAngleProperty : NestedProperty
    {
        private HPS.CurveAttributeKit kit;
        private bool set;
        private float _degrees;

        public CurveAttributeKitMaximumAngleProperty(
            HPS.CurveAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMaximumAngle(out _degrees);
            if (!set)
            {
                _degrees = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Degrees", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Degrees": e.Value = _degrees; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Degrees":
                {
                    _degrees = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMaximumAngle(_degrees);
            else
                kit.UnsetMaximumAngle();
        }
    }

    [ExpandableObject]
    public class CurveAttributeKitMaximumLengthProperty : NestedProperty
    {
        private HPS.CurveAttributeKit kit;
        private bool set;
        private float _length;

        public CurveAttributeKitMaximumLengthProperty(
            HPS.CurveAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMaximumLength(out _length);
            if (!set)
            {
                _length = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Length", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Length": e.Value = _length; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Length":
                {
                    _length = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMaximumLength(_length);
            else
                kit.UnsetMaximumLength();
        }
    }

    [DisplayName("CurveAttribute")]
    public class CurveAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.CurveAttributeKit kit;

        public CurveAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowCurveAttribute(out kit);

            Budget = new CurveAttributeKitBudgetProperty(kit);
            ContinuedBudget = new CurveAttributeKitContinuedBudgetProperty(kit);
            ViewDependent = new CurveAttributeKitViewDependentProperty(kit);
            MaximumDeviation = new CurveAttributeKitMaximumDeviationProperty(kit);
            MaximumAngle = new CurveAttributeKitMaximumAngleProperty(kit);
            MaximumLength = new CurveAttributeKitMaximumLengthProperty(kit);
        }

        [PropertyOrder(0)]
        public CurveAttributeKitBudgetProperty Budget { get; set; }

        [PropertyOrder(1)]
        public CurveAttributeKitContinuedBudgetProperty ContinuedBudget { get; set; }

        [PropertyOrder(2)]
        public CurveAttributeKitViewDependentProperty ViewDependent { get; set; }

        [PropertyOrder(3)]
        public CurveAttributeKitMaximumDeviationProperty MaximumDeviation { get; set; }

        [PropertyOrder(4)]
        public CurveAttributeKitMaximumAngleProperty MaximumAngle { get; set; }

        [PropertyOrder(5)]
        public CurveAttributeKitMaximumLengthProperty MaximumLength { get; set; }

        public void Apply()
        {
            key.UnsetCurveAttribute();
            key.SetCurveAttribute(kit);
        }
    }
    #endregion

    #region PBRMaterialKit
    [ExpandableObject]
    public class PBRMaterialKitBaseColorMapProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private string _texture_name;

        public PBRMaterialKitBaseColorMapProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBaseColorMap(out _texture_name);
            if (!set)
            {
                _texture_name = "texture_name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Texture Name", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Texture Name": e.Value = _texture_name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Texture Name":
                {
                    _texture_name = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBaseColorMap(_texture_name);
            else
                kit.UnsetBaseColorMap();
        }
    }

    [ExpandableObject]
    public class PBRMaterialKitNormalMapProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private string _texture_name;

        public PBRMaterialKitNormalMapProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowNormalMap(out _texture_name);
            if (!set)
            {
                _texture_name = "texture_name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Texture Name", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Texture Name": e.Value = _texture_name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Texture Name":
                {
                    _texture_name = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetNormalMap(_texture_name);
            else
                kit.UnsetNormalMap();
        }
    }

    [ExpandableObject]
    public class PBRMaterialKitEmissiveMapProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private string _texture_name;

        public PBRMaterialKitEmissiveMapProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowEmissiveMap(out _texture_name);
            if (!set)
            {
                _texture_name = "texture_name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Texture Name", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Texture Name": e.Value = _texture_name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Texture Name":
                {
                    _texture_name = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetEmissiveMap(_texture_name);
            else
                kit.UnsetEmissiveMap();
        }
    }

    [ExpandableObject]
    public class PBRMaterialKitMetalnessMapProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private string _texture_name;
        private HPS.Material.Texture.ChannelMapping _channel;

        public PBRMaterialKitMetalnessMapProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMetalnessMap(out _texture_name, out _channel);
            if (!set)
            {
                _texture_name = "texture_name";
                _channel = HPS.Material.Texture.ChannelMapping.Red;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Texture Name", typeof(string), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Channel", typeof(HPS.Material.Texture.ChannelMapping), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Texture Name": e.Value = _texture_name; break;
                case "Channel": e.Value = _channel; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Texture Name":
                {
                    _texture_name = (string)e.Value;
                }
                break;
                case "Channel":
                {
                    _channel = (HPS.Material.Texture.ChannelMapping)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMetalnessMap(_texture_name, _channel);
            else
                kit.UnsetMetalnessMap();
        }
    }

    [ExpandableObject]
    public class PBRMaterialKitRoughnessMapProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private string _texture_name;
        private HPS.Material.Texture.ChannelMapping _channel;

        public PBRMaterialKitRoughnessMapProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowRoughnessMap(out _texture_name, out _channel);
            if (!set)
            {
                _texture_name = "texture_name";
                _channel = HPS.Material.Texture.ChannelMapping.Red;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Texture Name", typeof(string), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Channel", typeof(HPS.Material.Texture.ChannelMapping), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Texture Name": e.Value = _texture_name; break;
                case "Channel": e.Value = _channel; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Texture Name":
                {
                    _texture_name = (string)e.Value;
                }
                break;
                case "Channel":
                {
                    _channel = (HPS.Material.Texture.ChannelMapping)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetRoughnessMap(_texture_name, _channel);
            else
                kit.UnsetRoughnessMap();
        }
    }

    [ExpandableObject]
    public class PBRMaterialKitOcclusionMapProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private string _texture_name;
        private HPS.Material.Texture.ChannelMapping _channel;

        public PBRMaterialKitOcclusionMapProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowOcclusionMap(out _texture_name, out _channel);
            if (!set)
            {
                _texture_name = "texture_name";
                _channel = HPS.Material.Texture.ChannelMapping.Red;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Texture Name", typeof(string), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Channel", typeof(HPS.Material.Texture.ChannelMapping), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Texture Name": e.Value = _texture_name; break;
                case "Channel": e.Value = _channel; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Texture Name":
                {
                    _texture_name = (string)e.Value;
                }
                break;
                case "Channel":
                {
                    _channel = (HPS.Material.Texture.ChannelMapping)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetOcclusionMap(_texture_name, _channel);
            else
                kit.UnsetOcclusionMap();
        }
    }

    [ExpandableObject]
    public class PBRMaterialKitBaseColorFactorProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private HPS.RGBAColor _color;
        private RGBAColorProperty _colorProperty;

        public PBRMaterialKitBaseColorFactorProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBaseColorFactor(out _color);
            if (!set)
            {
                _color = HPS.RGBAColor.Black();
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            _colorProperty = new RGBAColorProperty(_color, Update_color);
            Properties.Add(new PropertySpec("Color", typeof(RGBAColorProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Color": e.Value = _colorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Color":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBaseColorFactor(_color);
            else
                kit.UnsetBaseColorFactor();
        }

        private void Update_color(
            HPS.RGBAColor _color)
        {
            this._color = _color;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class PBRMaterialKitNormalFactorProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private float _factor;

        public PBRMaterialKitNormalFactorProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowNormalFactor(out _factor);
            if (!set)
            {
                _factor = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Factor", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Factor": e.Value = _factor; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Factor":
                {
                    _factor = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetNormalFactor(_factor);
            else
                kit.UnsetNormalFactor();
        }
    }

    [ExpandableObject]
    public class PBRMaterialKitMetalnessFactorProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private float _factor;

        public PBRMaterialKitMetalnessFactorProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMetalnessFactor(out _factor);
            if (!set)
            {
                _factor = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Factor", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Factor": e.Value = _factor; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Factor":
                {
                    _factor = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMetalnessFactor(_factor);
            else
                kit.UnsetMetalnessFactor();
        }
    }

    [ExpandableObject]
    public class PBRMaterialKitRoughnessFactorProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private float _factor;

        public PBRMaterialKitRoughnessFactorProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowRoughnessFactor(out _factor);
            if (!set)
            {
                _factor = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Factor", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Factor": e.Value = _factor; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Factor":
                {
                    _factor = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetRoughnessFactor(_factor);
            else
                kit.UnsetRoughnessFactor();
        }
    }

    [ExpandableObject]
    public class PBRMaterialKitOcclusionFactorProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private float _factor;

        public PBRMaterialKitOcclusionFactorProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowOcclusionFactor(out _factor);
            if (!set)
            {
                _factor = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Factor", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Factor": e.Value = _factor; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Factor":
                {
                    _factor = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetOcclusionFactor(_factor);
            else
                kit.UnsetOcclusionFactor();
        }
    }

    [ExpandableObject]
    public class PBRMaterialKitAlphaFactorProperty : NestedProperty
    {
        private HPS.PBRMaterialKit kit;
        private bool set;
        private float _factor;
        private bool _mask;

        public PBRMaterialKitAlphaFactorProperty(
            HPS.PBRMaterialKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowAlphaFactor(out _factor, out _mask);
            if (!set)
            {
                _factor = 0.0f;
                _mask = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Factor", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Mask", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Factor": e.Value = _factor; break;
                case "Mask": e.Value = _mask; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Factor":
                {
                    _factor = (float)e.Value;
                }
                break;
                case "Mask":
                {
                    _mask = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetAlphaFactor(_factor, _mask);
            else
                kit.UnsetAlphaFactor();
        }
    }

    [DisplayName("PBRMaterial")]
    public class PBRMaterialKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.PBRMaterialKit kit;

        public PBRMaterialKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowPBRMaterial(out kit);

            BaseColorMap = new PBRMaterialKitBaseColorMapProperty(kit);
            NormalMap = new PBRMaterialKitNormalMapProperty(kit);
            EmissiveMap = new PBRMaterialKitEmissiveMapProperty(kit);
            MetalnessMap = new PBRMaterialKitMetalnessMapProperty(kit);
            RoughnessMap = new PBRMaterialKitRoughnessMapProperty(kit);
            OcclusionMap = new PBRMaterialKitOcclusionMapProperty(kit);
            BaseColorFactor = new PBRMaterialKitBaseColorFactorProperty(kit);
            NormalFactor = new PBRMaterialKitNormalFactorProperty(kit);
            MetalnessFactor = new PBRMaterialKitMetalnessFactorProperty(kit);
            RoughnessFactor = new PBRMaterialKitRoughnessFactorProperty(kit);
            OcclusionFactor = new PBRMaterialKitOcclusionFactorProperty(kit);
            AlphaFactor = new PBRMaterialKitAlphaFactorProperty(kit);
        }

        [PropertyOrder(0)]
        public PBRMaterialKitBaseColorMapProperty BaseColorMap { get; set; }

        [PropertyOrder(1)]
        public PBRMaterialKitNormalMapProperty NormalMap { get; set; }

        [PropertyOrder(2)]
        public PBRMaterialKitEmissiveMapProperty EmissiveMap { get; set; }

        [PropertyOrder(3)]
        public PBRMaterialKitMetalnessMapProperty MetalnessMap { get; set; }

        [PropertyOrder(4)]
        public PBRMaterialKitRoughnessMapProperty RoughnessMap { get; set; }

        [PropertyOrder(5)]
        public PBRMaterialKitOcclusionMapProperty OcclusionMap { get; set; }

        [PropertyOrder(6)]
        public PBRMaterialKitBaseColorFactorProperty BaseColorFactor { get; set; }

        [PropertyOrder(7)]
        public PBRMaterialKitNormalFactorProperty NormalFactor { get; set; }

        [PropertyOrder(8)]
        public PBRMaterialKitMetalnessFactorProperty MetalnessFactor { get; set; }

        [PropertyOrder(9)]
        public PBRMaterialKitRoughnessFactorProperty RoughnessFactor { get; set; }

        [PropertyOrder(10)]
        public PBRMaterialKitOcclusionFactorProperty OcclusionFactor { get; set; }

        [PropertyOrder(11)]
        public PBRMaterialKitAlphaFactorProperty AlphaFactor { get; set; }

        public void Apply()
        {
            key.UnsetPBRMaterial();
            key.SetPBRMaterial(kit);
        }
    }
    #endregion

    #region MarkerKit
    [ExpandableObject]
    public class MarkerKitPointProperty : NestedProperty
    {
        private HPS.MarkerKit kit;
        private HPS.Point _point;
        private PointProperty _pointProperty;

        public MarkerKitPointProperty(
            HPS.MarkerKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowPoint(out _point);

            GetValue += Get;
            SetValue += Set;
            _pointProperty = new PointProperty(_point, Update_point);
            Properties.Add(new PropertySpec("Point", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Point": e.Value = _pointProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Point":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetPoint(_point);
        }

        private void Update_point(
            HPS.Point _point)
        {
            this._point = _point;
            UpdateKit();
        }
    }

    [DisplayName("Marker")]
    public class MarkerKitProperty : IRootProperty
    {
        private HPS.MarkerKey key;
        private HPS.MarkerKit kit;

        public MarkerKitProperty(
            HPS.MarkerKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Point = new MarkerKitPointProperty(kit);
            Priority = new PriorityProperty<HPS.MarkerKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.MarkerKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public MarkerKitPointProperty Point { get; set; }

        [PropertyOrder(1)]
        public PriorityProperty<HPS.MarkerKit> Priority { get; set; }

        [PropertyOrder(2)]
        public UserDataProperty<HPS.MarkerKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region DistantLightKit
    [ExpandableObject]
    public class DistantLightKitDirectionProperty : NestedProperty
    {
        private HPS.DistantLightKit kit;
        private HPS.Vector _vector;
        private VectorProperty _vectorProperty;

        public DistantLightKitDirectionProperty(
            HPS.DistantLightKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowDirection(out _vector);

            GetValue += Get;
            SetValue += Set;
            _vectorProperty = new VectorProperty(_vector, Update_vector);
            Properties.Add(new PropertySpec("Vector", typeof(VectorProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Vector": e.Value = _vectorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Vector":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetDirection(_vector);
        }

        private void Update_vector(
            HPS.Vector _vector)
        {
            this._vector = _vector;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class DistantLightKitCameraRelativeProperty : NestedProperty
    {
        private HPS.DistantLightKit kit;
        private bool _state;

        public DistantLightKitCameraRelativeProperty(
            HPS.DistantLightKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowCameraRelative(out _state);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetCameraRelative(_state);
        }
    }

    [DisplayName("DistantLight")]
    public class DistantLightKitProperty : IRootProperty
    {
        private HPS.DistantLightKey key;
        private HPS.DistantLightKit kit;

        public DistantLightKitProperty(
            HPS.DistantLightKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Direction = new DistantLightKitDirectionProperty(kit);
            Color = new DistantLightKitColorProperty(kit);
            CameraRelative = new DistantLightKitCameraRelativeProperty(kit);
            Priority = new PriorityProperty<HPS.DistantLightKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.DistantLightKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public DistantLightKitDirectionProperty Direction { get; set; }

        [PropertyOrder(1)]
        public DistantLightKitColorProperty Color { get; set; }

        [PropertyOrder(2)]
        public DistantLightKitCameraRelativeProperty CameraRelative { get; set; }

        [PropertyOrder(3)]
        public PriorityProperty<HPS.DistantLightKit> Priority { get; set; }

        [PropertyOrder(4)]
        public UserDataProperty<HPS.DistantLightKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region CuttingSectionAttributeKit
    [ExpandableObject]
    public class CuttingSectionAttributeKitCuttingLevelProperty : NestedProperty
    {
        private HPS.CuttingSectionAttributeKit kit;
        private bool set;
        private HPS.CuttingSection.CuttingLevel _level;

        public CuttingSectionAttributeKitCuttingLevelProperty(
            HPS.CuttingSectionAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowCuttingLevel(out _level);
            if (!set)
            {
                _level = HPS.CuttingSection.CuttingLevel.Global;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Level", typeof(HPS.CuttingSection.CuttingLevel), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Level": e.Value = _level; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Level":
                {
                    _level = (HPS.CuttingSection.CuttingLevel)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCuttingLevel(_level);
            else
                kit.UnsetCuttingLevel();
        }
    }

    [ExpandableObject]
    public class CuttingSectionAttributeKitCappingLevelProperty : NestedProperty
    {
        private HPS.CuttingSectionAttributeKit kit;
        private bool set;
        private HPS.CuttingSection.CappingLevel _level;

        public CuttingSectionAttributeKitCappingLevelProperty(
            HPS.CuttingSectionAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowCappingLevel(out _level);
            if (!set)
            {
                _level = HPS.CuttingSection.CappingLevel.Segment;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Level", typeof(HPS.CuttingSection.CappingLevel), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Level": e.Value = _level; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Level":
                {
                    _level = (HPS.CuttingSection.CappingLevel)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCappingLevel(_level);
            else
                kit.UnsetCappingLevel();
        }
    }

    [ExpandableObject]
    public class CuttingSectionAttributeKitCappingUsageProperty : NestedProperty
    {
        private HPS.CuttingSectionAttributeKit kit;
        private bool set;
        private HPS.CuttingSection.CappingUsage _usage;

        public CuttingSectionAttributeKitCappingUsageProperty(
            HPS.CuttingSectionAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowCappingUsage(out _usage);
            if (!set)
            {
                _usage = HPS.CuttingSection.CappingUsage.Visibility;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Usage", typeof(HPS.CuttingSection.CappingUsage), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Usage": e.Value = _usage; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Usage":
                {
                    _usage = (HPS.CuttingSection.CappingUsage)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCappingUsage(_usage);
            else
                kit.UnsetCappingUsage();
        }
    }

    [ExpandableObject]
    public class CuttingSectionAttributeKitMaterialPreferenceProperty : NestedProperty
    {
        private HPS.CuttingSectionAttributeKit kit;
        private bool set;
        private HPS.CuttingSection.MaterialPreference _preference;

        public CuttingSectionAttributeKitMaterialPreferenceProperty(
            HPS.CuttingSectionAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMaterialPreference(out _preference);
            if (!set)
            {
                _preference = HPS.CuttingSection.MaterialPreference.Explicit;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Preference", typeof(HPS.CuttingSection.MaterialPreference), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Preference": e.Value = _preference; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Preference":
                {
                    _preference = (HPS.CuttingSection.MaterialPreference)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMaterialPreference(_preference);
            else
                kit.UnsetMaterialPreference();
        }
    }

    [ExpandableObject]
    public class CuttingSectionAttributeKitEdgeWeightProperty : NestedProperty
    {
        private HPS.CuttingSectionAttributeKit kit;
        private bool set;
        private float _weight;
        private HPS.Line.SizeUnits _units;

        public CuttingSectionAttributeKitEdgeWeightProperty(
            HPS.CuttingSectionAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowEdgeWeight(out _weight, out _units);
            if (!set)
            {
                _weight = 0.0f;
                _units = HPS.Line.SizeUnits.ScaleFactor;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Weight", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Line.SizeUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Weight": e.Value = _weight; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Weight":
                {
                    _weight = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Line.SizeUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetEdgeWeight(_weight, _units);
            else
                kit.UnsetEdgeWeight();
        }
    }

    [ExpandableObject]
    public class CuttingSectionAttributeKitToleranceProperty : NestedProperty
    {
        private HPS.CuttingSectionAttributeKit kit;
        private bool set;
        private float _tolerance;
        private HPS.CuttingSection.ToleranceUnits _units;

        public CuttingSectionAttributeKitToleranceProperty(
            HPS.CuttingSectionAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowTolerance(out _tolerance, out _units);
            if (!set)
            {
                _tolerance = 0.0f;
                _units = HPS.CuttingSection.ToleranceUnits.WorldSpace;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Tolerance", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.CuttingSection.ToleranceUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Tolerance": e.Value = _tolerance; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Tolerance":
                {
                    _tolerance = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.CuttingSection.ToleranceUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetTolerance(_tolerance, _units);
            else
                kit.UnsetTolerance();
        }
    }

    [DisplayName("CuttingSectionAttribute")]
    public class CuttingSectionAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.CuttingSectionAttributeKit kit;

        public CuttingSectionAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowCuttingSectionAttribute(out kit);

            CuttingLevel = new CuttingSectionAttributeKitCuttingLevelProperty(kit);
            CappingLevel = new CuttingSectionAttributeKitCappingLevelProperty(kit);
            CappingUsage = new CuttingSectionAttributeKitCappingUsageProperty(kit);
            MaterialPreference = new CuttingSectionAttributeKitMaterialPreferenceProperty(kit);
            EdgeWeight = new CuttingSectionAttributeKitEdgeWeightProperty(kit);
            Tolerance = new CuttingSectionAttributeKitToleranceProperty(kit);
        }

        [PropertyOrder(0)]
        public CuttingSectionAttributeKitCuttingLevelProperty CuttingLevel { get; set; }

        [PropertyOrder(1)]
        public CuttingSectionAttributeKitCappingLevelProperty CappingLevel { get; set; }

        [PropertyOrder(2)]
        public CuttingSectionAttributeKitCappingUsageProperty CappingUsage { get; set; }

        [PropertyOrder(3)]
        public CuttingSectionAttributeKitMaterialPreferenceProperty MaterialPreference { get; set; }

        [PropertyOrder(4)]
        public CuttingSectionAttributeKitEdgeWeightProperty EdgeWeight { get; set; }

        [PropertyOrder(5)]
        public CuttingSectionAttributeKitToleranceProperty Tolerance { get; set; }

        public void Apply()
        {
            key.UnsetCuttingSectionAttribute();
            key.SetCuttingSectionAttribute(kit);
        }
    }
    #endregion

    #region CylinderAttributeKit
    [ExpandableObject]
    public class CylinderAttributeKitTessellationProperty : NestedProperty
    {
        private HPS.CylinderAttributeKit kit;
        private bool set;
        private ulong _facets;

        public CylinderAttributeKitTessellationProperty(
            HPS.CylinderAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowTessellation(out _facets);
            if (!set)
            {
                _facets = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Facets", typeof(ulong), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Facets": e.Value = _facets; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Facets":
                {
                    _facets = (ulong)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetTessellation(_facets);
            else
                kit.UnsetTessellation();
        }
    }

    [ExpandableObject]
    public class CylinderAttributeKitOrientationProperty : NestedProperty
    {
        private HPS.CylinderAttributeKit kit;
        private bool set;
        private HPS.Cylinder.Orientation _orientation;

        public CylinderAttributeKitOrientationProperty(
            HPS.CylinderAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowOrientation(out _orientation);
            if (!set)
            {
                _orientation = HPS.Cylinder.Orientation.Default;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Orientation", typeof(HPS.Cylinder.Orientation), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Orientation": e.Value = _orientation; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Orientation":
                {
                    _orientation = (HPS.Cylinder.Orientation)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetOrientation(_orientation);
            else
                kit.UnsetOrientation();
        }
    }

    [DisplayName("CylinderAttribute")]
    public class CylinderAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.CylinderAttributeKit kit;

        public CylinderAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowCylinderAttribute(out kit);

            Tessellation = new CylinderAttributeKitTessellationProperty(kit);
            Orientation = new CylinderAttributeKitOrientationProperty(kit);
        }

        [PropertyOrder(0)]
        public CylinderAttributeKitTessellationProperty Tessellation { get; set; }

        [PropertyOrder(1)]
        public CylinderAttributeKitOrientationProperty Orientation { get; set; }

        public void Apply()
        {
            key.UnsetCylinderAttribute();
            key.SetCylinderAttribute(kit);
        }
    }
    #endregion

    #region SphereKit
    [ExpandableObject]
    public class SphereKitCenterProperty : NestedProperty
    {
        private HPS.SphereKit kit;
        private HPS.Point _center;
        private PointProperty _centerProperty;

        public SphereKitCenterProperty(
            HPS.SphereKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowCenter(out _center);

            GetValue += Get;
            SetValue += Set;
            _centerProperty = new PointProperty(_center, Update_center);
            Properties.Add(new PropertySpec("Center", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Center": e.Value = _centerProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Center":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetCenter(_center);
        }

        private void Update_center(
            HPS.Point _center)
        {
            this._center = _center;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class SphereKitRadiusProperty : NestedProperty
    {
        private HPS.SphereKit kit;
        private float _radius;

        public SphereKitRadiusProperty(
            HPS.SphereKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowRadius(out _radius);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Radius", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Radius": e.Value = _radius; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Radius":
                {
                    _radius = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetRadius(_radius);
        }
    }

    [ExpandableObject]
    public class SphereKitBasisProperty : NestedProperty
    {
        private HPS.SphereKit kit;
        private HPS.Vector _vertical;
        private VectorProperty _verticalProperty;
        private HPS.Vector _horizontal;
        private VectorProperty _horizontalProperty;

        public SphereKitBasisProperty(
            HPS.SphereKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowBasis(out _vertical, out _horizontal);

            GetValue += Get;
            SetValue += Set;
            _verticalProperty = new VectorProperty(_vertical, Update_vertical);
            Properties.Add(new PropertySpec("Vertical", typeof(VectorProperty), null, expandable: true, triggersRefresh: false));
            _horizontalProperty = new VectorProperty(_horizontal, Update_horizontal);
            Properties.Add(new PropertySpec("Horizontal", typeof(VectorProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Vertical": e.Value = _verticalProperty; break;
                case "Horizontal": e.Value = _horizontalProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Vertical":
                {
                    // nothing to do
                }
                break;
                case "Horizontal":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetBasis(_vertical, _horizontal);
        }

        private void Update_vertical(
            HPS.Vector _vertical)
        {
            this._vertical = _vertical;
            UpdateKit();
        }

        private void Update_horizontal(
            HPS.Vector _horizontal)
        {
            this._horizontal = _horizontal;
            UpdateKit();
        }
    }

    [DisplayName("Sphere")]
    public class SphereKitProperty : IRootProperty
    {
        private HPS.SphereKey key;
        private HPS.SphereKit kit;

        public SphereKitProperty(
            HPS.SphereKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Center = new SphereKitCenterProperty(kit);
            Radius = new SphereKitRadiusProperty(kit);
            Basis = new SphereKitBasisProperty(kit);
            Priority = new PriorityProperty<HPS.SphereKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.SphereKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public SphereKitCenterProperty Center { get; set; }

        [PropertyOrder(1)]
        public SphereKitRadiusProperty Radius { get; set; }

        [PropertyOrder(2)]
        public SphereKitBasisProperty Basis { get; set; }

        [PropertyOrder(3)]
        public PriorityProperty<HPS.SphereKit> Priority { get; set; }

        [PropertyOrder(4)]
        public UserDataProperty<HPS.SphereKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region SphereAttributeKit
    [ExpandableObject]
    public class SphereAttributeKitTessellationProperty : NestedProperty
    {
        private HPS.SphereAttributeKit kit;
        private bool set;
        private ulong _facets;

        public SphereAttributeKitTessellationProperty(
            HPS.SphereAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowTessellation(out _facets);
            if (!set)
            {
                _facets = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Facets", typeof(ulong), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Facets": e.Value = _facets; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Facets":
                {
                    _facets = (ulong)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetTessellation(_facets);
            else
                kit.UnsetTessellation();
        }
    }

    [DisplayName("SphereAttribute")]
    public class SphereAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.SphereAttributeKit kit;

        public SphereAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowSphereAttribute(out kit);

            Tessellation = new SphereAttributeKitTessellationProperty(kit);
        }

        [PropertyOrder(0)]
        public SphereAttributeKitTessellationProperty Tessellation { get; set; }

        public void Apply()
        {
            key.UnsetSphereAttribute();
            key.SetSphereAttribute(kit);
        }
    }
    #endregion

    #region CircleKit
    [ExpandableObject]
    public class CircleKitCenterProperty : NestedProperty
    {
        private HPS.CircleKit kit;
        private HPS.Point _center;
        private PointProperty _centerProperty;

        public CircleKitCenterProperty(
            HPS.CircleKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowCenter(out _center);

            GetValue += Get;
            SetValue += Set;
            _centerProperty = new PointProperty(_center, Update_center);
            Properties.Add(new PropertySpec("Center", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Center": e.Value = _centerProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Center":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetCenter(_center);
        }

        private void Update_center(
            HPS.Point _center)
        {
            this._center = _center;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class CircleKitRadiusProperty : NestedProperty
    {
        private HPS.CircleKit kit;
        private float _radius;

        public CircleKitRadiusProperty(
            HPS.CircleKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowRadius(out _radius);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Radius", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Radius": e.Value = _radius; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Radius":
                {
                    _radius = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetRadius(_radius);
        }
    }

    [ExpandableObject]
    public class CircleKitNormalProperty : NestedProperty
    {
        private HPS.CircleKit kit;
        private HPS.Vector _normal;
        private VectorProperty _normalProperty;

        public CircleKitNormalProperty(
            HPS.CircleKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowNormal(out _normal);

            GetValue += Get;
            SetValue += Set;
            _normalProperty = new VectorProperty(_normal, Update_normal);
            Properties.Add(new PropertySpec("Normal", typeof(VectorProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Normal": e.Value = _normalProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Normal":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetNormal(_normal);
        }

        private void Update_normal(
            HPS.Vector _normal)
        {
            this._normal = _normal;
            UpdateKit();
        }
    }

    [DisplayName("Circle")]
    public class CircleKitProperty : IRootProperty
    {
        private HPS.CircleKey key;
        private HPS.CircleKit kit;

        public CircleKitProperty(
            HPS.CircleKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Center = new CircleKitCenterProperty(kit);
            Radius = new CircleKitRadiusProperty(kit);
            Normal = new CircleKitNormalProperty(kit);
            Priority = new PriorityProperty<HPS.CircleKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.CircleKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public CircleKitCenterProperty Center { get; set; }

        [PropertyOrder(1)]
        public CircleKitRadiusProperty Radius { get; set; }

        [PropertyOrder(2)]
        public CircleKitNormalProperty Normal { get; set; }

        [PropertyOrder(3)]
        public PriorityProperty<HPS.CircleKit> Priority { get; set; }

        [PropertyOrder(4)]
        public UserDataProperty<HPS.CircleKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region CircularArcKit
    [ExpandableObject]
    public class CircularArcKitStartProperty : NestedProperty
    {
        private HPS.CircularArcKit kit;
        private HPS.Point _start;
        private PointProperty _startProperty;

        public CircularArcKitStartProperty(
            HPS.CircularArcKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowStart(out _start);

            GetValue += Get;
            SetValue += Set;
            _startProperty = new PointProperty(_start, Update_start);
            Properties.Add(new PropertySpec("Start", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Start": e.Value = _startProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Start":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetStart(_start);
        }

        private void Update_start(
            HPS.Point _start)
        {
            this._start = _start;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class CircularArcKitMiddleProperty : NestedProperty
    {
        private HPS.CircularArcKit kit;
        private HPS.Point _middle;
        private PointProperty _middleProperty;

        public CircularArcKitMiddleProperty(
            HPS.CircularArcKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowMiddle(out _middle);

            GetValue += Get;
            SetValue += Set;
            _middleProperty = new PointProperty(_middle, Update_middle);
            Properties.Add(new PropertySpec("Middle", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Middle": e.Value = _middleProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Middle":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetMiddle(_middle);
        }

        private void Update_middle(
            HPS.Point _middle)
        {
            this._middle = _middle;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class CircularArcKitEndProperty : NestedProperty
    {
        private HPS.CircularArcKit kit;
        private HPS.Point _end;
        private PointProperty _endProperty;

        public CircularArcKitEndProperty(
            HPS.CircularArcKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowEnd(out _end);

            GetValue += Get;
            SetValue += Set;
            _endProperty = new PointProperty(_end, Update_end);
            Properties.Add(new PropertySpec("End", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "End": e.Value = _endProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "End":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetEnd(_end);
        }

        private void Update_end(
            HPS.Point _end)
        {
            this._end = _end;
            UpdateKit();
        }
    }

    [DisplayName("CircularArc")]
    public class CircularArcKitProperty : IRootProperty
    {
        private HPS.CircularArcKey key;
        private HPS.CircularArcKit kit;

        public CircularArcKitProperty(
            HPS.CircularArcKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Start = new CircularArcKitStartProperty(kit);
            Middle = new CircularArcKitMiddleProperty(kit);
            End = new CircularArcKitEndProperty(kit);
            Priority = new PriorityProperty<HPS.CircularArcKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.CircularArcKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public CircularArcKitStartProperty Start { get; set; }

        [PropertyOrder(1)]
        public CircularArcKitMiddleProperty Middle { get; set; }

        [PropertyOrder(2)]
        public CircularArcKitEndProperty End { get; set; }

        [PropertyOrder(3)]
        public PriorityProperty<HPS.CircularArcKit> Priority { get; set; }

        [PropertyOrder(4)]
        public UserDataProperty<HPS.CircularArcKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region CircularWedgeKit
    [ExpandableObject]
    public class CircularWedgeKitStartProperty : NestedProperty
    {
        private HPS.CircularWedgeKit kit;
        private HPS.Point _start;
        private PointProperty _startProperty;

        public CircularWedgeKitStartProperty(
            HPS.CircularWedgeKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowStart(out _start);

            GetValue += Get;
            SetValue += Set;
            _startProperty = new PointProperty(_start, Update_start);
            Properties.Add(new PropertySpec("Start", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Start": e.Value = _startProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Start":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetStart(_start);
        }

        private void Update_start(
            HPS.Point _start)
        {
            this._start = _start;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class CircularWedgeKitMiddleProperty : NestedProperty
    {
        private HPS.CircularWedgeKit kit;
        private HPS.Point _middle;
        private PointProperty _middleProperty;

        public CircularWedgeKitMiddleProperty(
            HPS.CircularWedgeKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowMiddle(out _middle);

            GetValue += Get;
            SetValue += Set;
            _middleProperty = new PointProperty(_middle, Update_middle);
            Properties.Add(new PropertySpec("Middle", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Middle": e.Value = _middleProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Middle":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetMiddle(_middle);
        }

        private void Update_middle(
            HPS.Point _middle)
        {
            this._middle = _middle;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class CircularWedgeKitEndProperty : NestedProperty
    {
        private HPS.CircularWedgeKit kit;
        private HPS.Point _end;
        private PointProperty _endProperty;

        public CircularWedgeKitEndProperty(
            HPS.CircularWedgeKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowEnd(out _end);

            GetValue += Get;
            SetValue += Set;
            _endProperty = new PointProperty(_end, Update_end);
            Properties.Add(new PropertySpec("End", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "End": e.Value = _endProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "End":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetEnd(_end);
        }

        private void Update_end(
            HPS.Point _end)
        {
            this._end = _end;
            UpdateKit();
        }
    }

    [DisplayName("CircularWedge")]
    public class CircularWedgeKitProperty : IRootProperty
    {
        private HPS.CircularWedgeKey key;
        private HPS.CircularWedgeKit kit;

        public CircularWedgeKitProperty(
            HPS.CircularWedgeKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Start = new CircularWedgeKitStartProperty(kit);
            Middle = new CircularWedgeKitMiddleProperty(kit);
            End = new CircularWedgeKitEndProperty(kit);
            Priority = new PriorityProperty<HPS.CircularWedgeKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.CircularWedgeKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public CircularWedgeKitStartProperty Start { get; set; }

        [PropertyOrder(1)]
        public CircularWedgeKitMiddleProperty Middle { get; set; }

        [PropertyOrder(2)]
        public CircularWedgeKitEndProperty End { get; set; }

        [PropertyOrder(3)]
        public PriorityProperty<HPS.CircularWedgeKit> Priority { get; set; }

        [PropertyOrder(4)]
        public UserDataProperty<HPS.CircularWedgeKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region InfiniteLineKit
    [ExpandableObject]
    public class InfiniteLineKitFirstProperty : NestedProperty
    {
        private HPS.InfiniteLineKit kit;
        private HPS.Point _first;
        private PointProperty _firstProperty;

        public InfiniteLineKitFirstProperty(
            HPS.InfiniteLineKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowFirst(out _first);

            GetValue += Get;
            SetValue += Set;
            _firstProperty = new PointProperty(_first, Update_first);
            Properties.Add(new PropertySpec("First", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "First": e.Value = _firstProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "First":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetFirst(_first);
        }

        private void Update_first(
            HPS.Point _first)
        {
            this._first = _first;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class InfiniteLineKitSecondProperty : NestedProperty
    {
        private HPS.InfiniteLineKit kit;
        private HPS.Point _second;
        private PointProperty _secondProperty;

        public InfiniteLineKitSecondProperty(
            HPS.InfiniteLineKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowSecond(out _second);

            GetValue += Get;
            SetValue += Set;
            _secondProperty = new PointProperty(_second, Update_second);
            Properties.Add(new PropertySpec("Second", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Second": e.Value = _secondProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Second":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetSecond(_second);
        }

        private void Update_second(
            HPS.Point _second)
        {
            this._second = _second;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class InfiniteLineKitTypeProperty : NestedProperty
    {
        private HPS.InfiniteLineKit kit;
        private HPS.InfiniteLine.Type _type;

        public InfiniteLineKitTypeProperty(
            HPS.InfiniteLineKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowType(out _type);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Type", typeof(HPS.InfiniteLine.Type), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type": e.Value = _type; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type":
                {
                    _type = (HPS.InfiniteLine.Type)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetType(_type);
        }
    }

    [DisplayName("InfiniteLine")]
    public class InfiniteLineKitProperty : IRootProperty
    {
        private HPS.InfiniteLineKey key;
        private HPS.InfiniteLineKit kit;

        public InfiniteLineKitProperty(
            HPS.InfiniteLineKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            First = new InfiniteLineKitFirstProperty(kit);
            Second = new InfiniteLineKitSecondProperty(kit);
            Type = new InfiniteLineKitTypeProperty(kit);
            Priority = new PriorityProperty<HPS.InfiniteLineKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.InfiniteLineKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public InfiniteLineKitFirstProperty First { get; set; }

        [PropertyOrder(1)]
        public InfiniteLineKitSecondProperty Second { get; set; }

        [PropertyOrder(2)]
        public InfiniteLineKitTypeProperty Type { get; set; }

        [PropertyOrder(3)]
        public PriorityProperty<HPS.InfiniteLineKit> Priority { get; set; }

        [PropertyOrder(4)]
        public UserDataProperty<HPS.InfiniteLineKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region SpotlightKit
    [ExpandableObject]
    public class SpotlightKitPositionProperty : NestedProperty
    {
        private HPS.SpotlightKit kit;
        private HPS.Point _position;
        private PointProperty _positionProperty;

        public SpotlightKitPositionProperty(
            HPS.SpotlightKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowPosition(out _position);

            GetValue += Get;
            SetValue += Set;
            _positionProperty = new PointProperty(_position, Update_position);
            Properties.Add(new PropertySpec("Position", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Position": e.Value = _positionProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Position":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetPosition(_position);
        }

        private void Update_position(
            HPS.Point _position)
        {
            this._position = _position;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class SpotlightKitTargetProperty : NestedProperty
    {
        private HPS.SpotlightKit kit;
        private HPS.Point _target;
        private PointProperty _targetProperty;

        public SpotlightKitTargetProperty(
            HPS.SpotlightKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowTarget(out _target);

            GetValue += Get;
            SetValue += Set;
            _targetProperty = new PointProperty(_target, Update_target);
            Properties.Add(new PropertySpec("Target", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Target": e.Value = _targetProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Target":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetTarget(_target);
        }

        private void Update_target(
            HPS.Point _target)
        {
            this._target = _target;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class SpotlightKitOuterConeProperty : NestedProperty
    {
        private HPS.SpotlightKit kit;
        private float _size;
        private HPS.Spotlight.OuterConeUnits _units;

        public SpotlightKitOuterConeProperty(
            HPS.SpotlightKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowOuterCone(out _size, out _units);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Size", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Spotlight.OuterConeUnits), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Size": e.Value = _size; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Size":
                {
                    _size = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Spotlight.OuterConeUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetOuterCone(_size, _units);
        }
    }

    [ExpandableObject]
    public class SpotlightKitInnerConeProperty : NestedProperty
    {
        private HPS.SpotlightKit kit;
        private float _size;
        private HPS.Spotlight.InnerConeUnits _units;

        public SpotlightKitInnerConeProperty(
            HPS.SpotlightKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowInnerCone(out _size, out _units);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Size", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Spotlight.InnerConeUnits), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Size": e.Value = _size; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Size":
                {
                    _size = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Spotlight.InnerConeUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetInnerCone(_size, _units);
        }
    }

    [ExpandableObject]
    public class SpotlightKitConcentrationProperty : NestedProperty
    {
        private HPS.SpotlightKit kit;
        private float _concentration;

        public SpotlightKitConcentrationProperty(
            HPS.SpotlightKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowConcentration(out _concentration);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Concentration", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Concentration": e.Value = _concentration; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Concentration":
                {
                    _concentration = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetConcentration(_concentration);
        }
    }

    [ExpandableObject]
    public class SpotlightKitCameraRelativeProperty : NestedProperty
    {
        private HPS.SpotlightKit kit;
        private bool _state;

        public SpotlightKitCameraRelativeProperty(
            HPS.SpotlightKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowCameraRelative(out _state);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetCameraRelative(_state);
        }
    }

    [DisplayName("Spotlight")]
    public class SpotlightKitProperty : IRootProperty
    {
        private HPS.SpotlightKey key;
        private HPS.SpotlightKit kit;

        public SpotlightKitProperty(
            HPS.SpotlightKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Position = new SpotlightKitPositionProperty(kit);
            Target = new SpotlightKitTargetProperty(kit);
            Color = new SpotlightKitColorProperty(kit);
            OuterCone = new SpotlightKitOuterConeProperty(kit);
            InnerCone = new SpotlightKitInnerConeProperty(kit);
            Concentration = new SpotlightKitConcentrationProperty(kit);
            CameraRelative = new SpotlightKitCameraRelativeProperty(kit);
            Priority = new PriorityProperty<HPS.SpotlightKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.SpotlightKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public SpotlightKitPositionProperty Position { get; set; }

        [PropertyOrder(1)]
        public SpotlightKitTargetProperty Target { get; set; }

        [PropertyOrder(2)]
        public SpotlightKitColorProperty Color { get; set; }

        [PropertyOrder(3)]
        public SpotlightKitOuterConeProperty OuterCone { get; set; }

        [PropertyOrder(4)]
        public SpotlightKitInnerConeProperty InnerCone { get; set; }

        [PropertyOrder(5)]
        public SpotlightKitConcentrationProperty Concentration { get; set; }

        [PropertyOrder(6)]
        public SpotlightKitCameraRelativeProperty CameraRelative { get; set; }

        [PropertyOrder(7)]
        public PriorityProperty<HPS.SpotlightKit> Priority { get; set; }

        [PropertyOrder(8)]
        public UserDataProperty<HPS.SpotlightKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region EllipseKit
    [ExpandableObject]
    public class EllipseKitCenterProperty : NestedProperty
    {
        private HPS.EllipseKit kit;
        private HPS.Point _center;
        private PointProperty _centerProperty;

        public EllipseKitCenterProperty(
            HPS.EllipseKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowCenter(out _center);

            GetValue += Get;
            SetValue += Set;
            _centerProperty = new PointProperty(_center, Update_center);
            Properties.Add(new PropertySpec("Center", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Center": e.Value = _centerProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Center":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetCenter(_center);
        }

        private void Update_center(
            HPS.Point _center)
        {
            this._center = _center;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class EllipseKitMajorProperty : NestedProperty
    {
        private HPS.EllipseKit kit;
        private HPS.Point _major;
        private PointProperty _majorProperty;

        public EllipseKitMajorProperty(
            HPS.EllipseKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowMajor(out _major);

            GetValue += Get;
            SetValue += Set;
            _majorProperty = new PointProperty(_major, Update_major);
            Properties.Add(new PropertySpec("Major", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Major": e.Value = _majorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Major":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetMajor(_major);
        }

        private void Update_major(
            HPS.Point _major)
        {
            this._major = _major;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class EllipseKitMinorProperty : NestedProperty
    {
        private HPS.EllipseKit kit;
        private HPS.Point _minor;
        private PointProperty _minorProperty;

        public EllipseKitMinorProperty(
            HPS.EllipseKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowMinor(out _minor);

            GetValue += Get;
            SetValue += Set;
            _minorProperty = new PointProperty(_minor, Update_minor);
            Properties.Add(new PropertySpec("Minor", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Minor": e.Value = _minorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Minor":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetMinor(_minor);
        }

        private void Update_minor(
            HPS.Point _minor)
        {
            this._minor = _minor;
            UpdateKit();
        }
    }

    [DisplayName("Ellipse")]
    public class EllipseKitProperty : IRootProperty
    {
        private HPS.EllipseKey key;
        private HPS.EllipseKit kit;

        public EllipseKitProperty(
            HPS.EllipseKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Center = new EllipseKitCenterProperty(kit);
            Major = new EllipseKitMajorProperty(kit);
            Minor = new EllipseKitMinorProperty(kit);
            Priority = new PriorityProperty<HPS.EllipseKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.EllipseKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public EllipseKitCenterProperty Center { get; set; }

        [PropertyOrder(1)]
        public EllipseKitMajorProperty Major { get; set; }

        [PropertyOrder(2)]
        public EllipseKitMinorProperty Minor { get; set; }

        [PropertyOrder(3)]
        public PriorityProperty<HPS.EllipseKit> Priority { get; set; }

        [PropertyOrder(4)]
        public UserDataProperty<HPS.EllipseKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region EllipticalArcKit
    [ExpandableObject]
    public class EllipticalArcKitCenterProperty : NestedProperty
    {
        private HPS.EllipticalArcKit kit;
        private HPS.Point _center;
        private PointProperty _centerProperty;

        public EllipticalArcKitCenterProperty(
            HPS.EllipticalArcKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowCenter(out _center);

            GetValue += Get;
            SetValue += Set;
            _centerProperty = new PointProperty(_center, Update_center);
            Properties.Add(new PropertySpec("Center", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Center": e.Value = _centerProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Center":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetCenter(_center);
        }

        private void Update_center(
            HPS.Point _center)
        {
            this._center = _center;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class EllipticalArcKitMajorProperty : NestedProperty
    {
        private HPS.EllipticalArcKit kit;
        private HPS.Point _major;
        private PointProperty _majorProperty;

        public EllipticalArcKitMajorProperty(
            HPS.EllipticalArcKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowMajor(out _major);

            GetValue += Get;
            SetValue += Set;
            _majorProperty = new PointProperty(_major, Update_major);
            Properties.Add(new PropertySpec("Major", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Major": e.Value = _majorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Major":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetMajor(_major);
        }

        private void Update_major(
            HPS.Point _major)
        {
            this._major = _major;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class EllipticalArcKitMinorProperty : NestedProperty
    {
        private HPS.EllipticalArcKit kit;
        private HPS.Point _minor;
        private PointProperty _minorProperty;

        public EllipticalArcKitMinorProperty(
            HPS.EllipticalArcKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowMinor(out _minor);

            GetValue += Get;
            SetValue += Set;
            _minorProperty = new PointProperty(_minor, Update_minor);
            Properties.Add(new PropertySpec("Minor", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Minor": e.Value = _minorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Minor":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetMinor(_minor);
        }

        private void Update_minor(
            HPS.Point _minor)
        {
            this._minor = _minor;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class EllipticalArcKitStartProperty : NestedProperty
    {
        private HPS.EllipticalArcKit kit;
        private float _start;

        public EllipticalArcKitStartProperty(
            HPS.EllipticalArcKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowStart(out _start);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Start", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Start": e.Value = _start; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Start":
                {
                    _start = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetStart(_start);
        }
    }

    [ExpandableObject]
    public class EllipticalArcKitEndProperty : NestedProperty
    {
        private HPS.EllipticalArcKit kit;
        private float _end;

        public EllipticalArcKitEndProperty(
            HPS.EllipticalArcKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowEnd(out _end);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("End", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "End": e.Value = _end; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "End":
                {
                    _end = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetEnd(_end);
        }
    }

    [DisplayName("EllipticalArc")]
    public class EllipticalArcKitProperty : IRootProperty
    {
        private HPS.EllipticalArcKey key;
        private HPS.EllipticalArcKit kit;

        public EllipticalArcKitProperty(
            HPS.EllipticalArcKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Center = new EllipticalArcKitCenterProperty(kit);
            Major = new EllipticalArcKitMajorProperty(kit);
            Minor = new EllipticalArcKitMinorProperty(kit);
            Start = new EllipticalArcKitStartProperty(kit);
            End = new EllipticalArcKitEndProperty(kit);
            Priority = new PriorityProperty<HPS.EllipticalArcKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.EllipticalArcKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public EllipticalArcKitCenterProperty Center { get; set; }

        [PropertyOrder(1)]
        public EllipticalArcKitMajorProperty Major { get; set; }

        [PropertyOrder(2)]
        public EllipticalArcKitMinorProperty Minor { get; set; }

        [PropertyOrder(3)]
        public EllipticalArcKitStartProperty Start { get; set; }

        [PropertyOrder(4)]
        public EllipticalArcKitEndProperty End { get; set; }

        [PropertyOrder(5)]
        public PriorityProperty<HPS.EllipticalArcKit> Priority { get; set; }

        [PropertyOrder(6)]
        public UserDataProperty<HPS.EllipticalArcKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

    #region NURBSSurfaceAttributeKit
    [ExpandableObject]
    public class NURBSSurfaceAttributeKitBudgetProperty : NestedProperty
    {
        private HPS.NURBSSurfaceAttributeKit kit;
        private bool set;
        private ulong _budget;

        public NURBSSurfaceAttributeKitBudgetProperty(
            HPS.NURBSSurfaceAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBudget(out _budget);
            if (!set)
            {
                _budget = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Budget", typeof(ulong), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Budget": e.Value = _budget; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Budget":
                {
                    _budget = (ulong)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBudget(_budget);
            else
                kit.UnsetBudget();
        }
    }

    [ExpandableObject]
    public class NURBSSurfaceAttributeKitMaximumDeviationProperty : NestedProperty
    {
        private HPS.NURBSSurfaceAttributeKit kit;
        private bool set;
        private float _deviation;

        public NURBSSurfaceAttributeKitMaximumDeviationProperty(
            HPS.NURBSSurfaceAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMaximumDeviation(out _deviation);
            if (!set)
            {
                _deviation = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Deviation", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Deviation": e.Value = _deviation; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Deviation":
                {
                    _deviation = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMaximumDeviation(_deviation);
            else
                kit.UnsetMaximumDeviation();
        }
    }

    [ExpandableObject]
    public class NURBSSurfaceAttributeKitMaximumAngleProperty : NestedProperty
    {
        private HPS.NURBSSurfaceAttributeKit kit;
        private bool set;
        private float _degrees;

        public NURBSSurfaceAttributeKitMaximumAngleProperty(
            HPS.NURBSSurfaceAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMaximumAngle(out _degrees);
            if (!set)
            {
                _degrees = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Degrees", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Degrees": e.Value = _degrees; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Degrees":
                {
                    _degrees = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMaximumAngle(_degrees);
            else
                kit.UnsetMaximumAngle();
        }
    }

    [ExpandableObject]
    public class NURBSSurfaceAttributeKitMaximumWidthProperty : NestedProperty
    {
        private HPS.NURBSSurfaceAttributeKit kit;
        private bool set;
        private float _width;

        public NURBSSurfaceAttributeKitMaximumWidthProperty(
            HPS.NURBSSurfaceAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMaximumWidth(out _width);
            if (!set)
            {
                _width = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Width", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Width": e.Value = _width; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Width":
                {
                    _width = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMaximumWidth(_width);
            else
                kit.UnsetMaximumWidth();
        }
    }

    [ExpandableObject]
    public class NURBSSurfaceAttributeKitTrimBudgetProperty : NestedProperty
    {
        private HPS.NURBSSurfaceAttributeKit kit;
        private bool set;
        private ulong _budget;

        public NURBSSurfaceAttributeKitTrimBudgetProperty(
            HPS.NURBSSurfaceAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowTrimBudget(out _budget);
            if (!set)
            {
                _budget = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Budget", typeof(ulong), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Budget": e.Value = _budget; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Budget":
                {
                    _budget = (ulong)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetTrimBudget(_budget);
            else
                kit.UnsetTrimBudget();
        }
    }

    [ExpandableObject]
    public class NURBSSurfaceAttributeKitMaximumTrimDeviationProperty : NestedProperty
    {
        private HPS.NURBSSurfaceAttributeKit kit;
        private bool set;
        private float _deviation;

        public NURBSSurfaceAttributeKitMaximumTrimDeviationProperty(
            HPS.NURBSSurfaceAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMaximumTrimDeviation(out _deviation);
            if (!set)
            {
                _deviation = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Deviation", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Deviation": e.Value = _deviation; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Deviation":
                {
                    _deviation = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMaximumTrimDeviation(_deviation);
            else
                kit.UnsetMaximumTrimDeviation();
        }
    }

    [DisplayName("NURBSSurfaceAttribute")]
    public class NURBSSurfaceAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.NURBSSurfaceAttributeKit kit;

        public NURBSSurfaceAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowNURBSSurfaceAttribute(out kit);

            Budget = new NURBSSurfaceAttributeKitBudgetProperty(kit);
            MaximumDeviation = new NURBSSurfaceAttributeKitMaximumDeviationProperty(kit);
            MaximumAngle = new NURBSSurfaceAttributeKitMaximumAngleProperty(kit);
            MaximumWidth = new NURBSSurfaceAttributeKitMaximumWidthProperty(kit);
            TrimBudget = new NURBSSurfaceAttributeKitTrimBudgetProperty(kit);
            MaximumTrimDeviation = new NURBSSurfaceAttributeKitMaximumTrimDeviationProperty(kit);
        }

        [PropertyOrder(0)]
        public NURBSSurfaceAttributeKitBudgetProperty Budget { get; set; }

        [PropertyOrder(1)]
        public NURBSSurfaceAttributeKitMaximumDeviationProperty MaximumDeviation { get; set; }

        [PropertyOrder(2)]
        public NURBSSurfaceAttributeKitMaximumAngleProperty MaximumAngle { get; set; }

        [PropertyOrder(3)]
        public NURBSSurfaceAttributeKitMaximumWidthProperty MaximumWidth { get; set; }

        [PropertyOrder(4)]
        public NURBSSurfaceAttributeKitTrimBudgetProperty TrimBudget { get; set; }

        [PropertyOrder(5)]
        public NURBSSurfaceAttributeKitMaximumTrimDeviationProperty MaximumTrimDeviation { get; set; }

        public void Apply()
        {
            key.UnsetNURBSSurfaceAttribute();
            key.SetNURBSSurfaceAttribute(kit);
        }
    }
    #endregion

    #region PerformanceKit
    [ExpandableObject]
    public class PerformanceKitDisplayListsProperty : NestedProperty
    {
        private HPS.PerformanceKit kit;
        private bool set;
        private HPS.Performance.DisplayLists _display_list;

        public PerformanceKitDisplayListsProperty(
            HPS.PerformanceKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDisplayLists(out _display_list);
            if (!set)
            {
                _display_list = HPS.Performance.DisplayLists.Segment;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Display List", typeof(HPS.Performance.DisplayLists), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Display List": e.Value = _display_list; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Display List":
                {
                    _display_list = (HPS.Performance.DisplayLists)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDisplayLists(_display_list);
            else
                kit.UnsetDisplayLists();
        }
    }

    [ExpandableObject]
    public class PerformanceKitTextHardwareAccelerationProperty : NestedProperty
    {
        private HPS.PerformanceKit kit;
        private bool set;
        private bool _state;

        public PerformanceKitTextHardwareAccelerationProperty(
            HPS.PerformanceKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowTextHardwareAcceleration(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetTextHardwareAcceleration(_state);
            else
                kit.UnsetTextHardwareAcceleration();
        }
    }

    [ExpandableObject]
    public class PerformanceKitStaticModelProperty : NestedProperty
    {
        private HPS.PerformanceKit kit;
        private bool set;
        private HPS.Performance.StaticModel _model_type;

        public PerformanceKitStaticModelProperty(
            HPS.PerformanceKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowStaticModel(out _model_type);
            if (!set)
            {
                _model_type = HPS.Performance.StaticModel.Attribute;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Model Type", typeof(HPS.Performance.StaticModel), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Model Type": e.Value = _model_type; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Model Type":
                {
                    _model_type = (HPS.Performance.StaticModel)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetStaticModel(_model_type);
            else
                kit.UnsetStaticModel();
        }
    }

    [ExpandableObject]
    public class PerformanceKitStaticConditionsProperty : NestedProperty
    {
        private HPS.PerformanceKit kit;
        private bool set;
        private HPS.Performance.StaticConditions _conditions;

        public PerformanceKitStaticConditionsProperty(
            HPS.PerformanceKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowStaticConditions(out _conditions);
            if (!set)
            {
                _conditions = HPS.Performance.StaticConditions.Independent;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Conditions", typeof(HPS.Performance.StaticConditions), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Conditions": e.Value = _conditions; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Conditions":
                {
                    _conditions = (HPS.Performance.StaticConditions)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetStaticConditions(_conditions);
            else
                kit.UnsetStaticConditions();
        }
    }

    [DisplayName("Performance")]
    public class PerformanceKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.PerformanceKit kit;

        public PerformanceKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowPerformance(out kit);

            DisplayLists = new PerformanceKitDisplayListsProperty(kit);
            TextHardwareAcceleration = new PerformanceKitTextHardwareAccelerationProperty(kit);
            StaticModel = new PerformanceKitStaticModelProperty(kit);
            StaticConditions = new PerformanceKitStaticConditionsProperty(kit);
        }

        [PropertyOrder(0)]
        public PerformanceKitDisplayListsProperty DisplayLists { get; set; }

        [PropertyOrder(1)]
        public PerformanceKitTextHardwareAccelerationProperty TextHardwareAcceleration { get; set; }

        [PropertyOrder(2)]
        public PerformanceKitStaticModelProperty StaticModel { get; set; }

        [PropertyOrder(3)]
        public PerformanceKitStaticConditionsProperty StaticConditions { get; set; }

        public void Apply()
        {
            key.UnsetPerformance();
            key.SetPerformance(kit);
        }
    }
    #endregion

    #region HiddenLineAttributeKit
    [ExpandableObject]
    public class HiddenLineAttributeKitAlgorithmProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private HPS.HiddenLine.Algorithm _algorithm;

        public HiddenLineAttributeKitAlgorithmProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowAlgorithm(out _algorithm);
            if (!set)
            {
                _algorithm = HPS.HiddenLine.Algorithm.ZBuffer;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Algorithm", typeof(HPS.HiddenLine.Algorithm), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Algorithm": e.Value = _algorithm; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Algorithm":
                {
                    _algorithm = (HPS.HiddenLine.Algorithm)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetAlgorithm(_algorithm);
            else
                kit.UnsetAlgorithm();
        }
    }

    [ExpandableObject]
    public class HiddenLineAttributeKitColorProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private HPS.RGBAColor _color;
        private RGBAColorProperty _colorProperty;

        public HiddenLineAttributeKitColorProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowColor(out _color);
            if (!set)
            {
                _color = HPS.RGBAColor.Black();
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            _colorProperty = new RGBAColorProperty(_color, Update_color);
            Properties.Add(new PropertySpec("Color", typeof(RGBAColorProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Color": e.Value = _colorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Color":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetColor(_color);
            else
                kit.UnsetColor();
        }

        private void Update_color(
            HPS.RGBAColor _color)
        {
            this._color = _color;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class HiddenLineAttributeKitDimFactorProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private float _zero_to_one;

        public HiddenLineAttributeKitDimFactorProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDimFactor(out _zero_to_one);
            if (!set)
            {
                _zero_to_one = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Zero To One", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Zero To One": e.Value = _zero_to_one; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Zero To One":
                {
                    _zero_to_one = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDimFactor(_zero_to_one);
            else
                kit.UnsetDimFactor();
        }
    }

    [ExpandableObject]
    public class HiddenLineAttributeKitFaceDisplacementProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private float _buckets;

        public HiddenLineAttributeKitFaceDisplacementProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowFaceDisplacement(out _buckets);
            if (!set)
            {
                _buckets = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Buckets", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Buckets": e.Value = _buckets; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Buckets":
                {
                    _buckets = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetFaceDisplacement(_buckets);
            else
                kit.UnsetFaceDisplacement();
        }
    }

    [ExpandableObject]
    public class HiddenLineAttributeKitLinePatternProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private string _pattern;

        public HiddenLineAttributeKitLinePatternProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowLinePattern(out _pattern);
            if (!set)
            {
                _pattern = "pattern";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Pattern", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Pattern": e.Value = _pattern; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Pattern":
                {
                    _pattern = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetLinePattern(_pattern);
            else
                kit.UnsetLinePattern();
        }
    }

    [ExpandableObject]
    public class HiddenLineAttributeKitLineSortingProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private bool _state;
        private float _threshold;
        private HPS.Line.SizeUnits _units;

        public HiddenLineAttributeKitLineSortingProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowLineSorting(out _state, out _threshold, out _units);
            if (!set)
            {
                _state = true;
                _threshold = 0.0f;
                _units = HPS.Line.SizeUnits.ScaleFactor;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Threshold", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Line.SizeUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Threshold": e.Value = _threshold; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
                case "Threshold":
                {
                    _threshold = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Line.SizeUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetLineSorting(_state, _threshold, _units);
            else
                kit.UnsetLineSorting();
        }
    }

    [ExpandableObject]
    public class HiddenLineAttributeKitRenderFacesProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private bool _state;

        public HiddenLineAttributeKitRenderFacesProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowRenderFaces(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetRenderFaces(_state);
            else
                kit.UnsetRenderFaces();
        }
    }

    [ExpandableObject]
    public class HiddenLineAttributeKitRenderTextProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private bool _state;

        public HiddenLineAttributeKitRenderTextProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowRenderText(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetRenderText(_state);
            else
                kit.UnsetRenderText();
        }
    }

    [ExpandableObject]
    public class HiddenLineAttributeKitSilhouetteCleanupProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private bool _state;

        public HiddenLineAttributeKitSilhouetteCleanupProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSilhouetteCleanup(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSilhouetteCleanup(_state);
            else
                kit.UnsetSilhouetteCleanup();
        }
    }

    [ExpandableObject]
    public class HiddenLineAttributeKitTransparencyCutoffProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private float _zero_to_one;

        public HiddenLineAttributeKitTransparencyCutoffProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowTransparencyCutoff(out _zero_to_one);
            if (!set)
            {
                _zero_to_one = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Zero To One", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Zero To One": e.Value = _zero_to_one; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Zero To One":
                {
                    _zero_to_one = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetTransparencyCutoff(_zero_to_one);
            else
                kit.UnsetTransparencyCutoff();
        }
    }

    [ExpandableObject]
    public class HiddenLineAttributeKitVisibilityProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private bool _state;

        public HiddenLineAttributeKitVisibilityProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowVisibility(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVisibility(_state);
            else
                kit.UnsetVisibility();
        }
    }

    [ExpandableObject]
    public class HiddenLineAttributeKitWeightProperty : NestedProperty
    {
        private HPS.HiddenLineAttributeKit kit;
        private bool set;
        private float _weight;
        private HPS.Line.SizeUnits _units;

        public HiddenLineAttributeKitWeightProperty(
            HPS.HiddenLineAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowWeight(out _weight, out _units);
            if (!set)
            {
                _weight = 0.0f;
                _units = HPS.Line.SizeUnits.ScaleFactor;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Weight", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Line.SizeUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Weight": e.Value = _weight; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Weight":
                {
                    _weight = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Line.SizeUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetWeight(_weight, _units);
            else
                kit.UnsetWeight();
        }
    }

    [DisplayName("HiddenLineAttribute")]
    public class HiddenLineAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.HiddenLineAttributeKit kit;

        public HiddenLineAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowHiddenLineAttribute(out kit);

            Algorithm = new HiddenLineAttributeKitAlgorithmProperty(kit);
            Color = new HiddenLineAttributeKitColorProperty(kit);
            DimFactor = new HiddenLineAttributeKitDimFactorProperty(kit);
            FaceDisplacement = new HiddenLineAttributeKitFaceDisplacementProperty(kit);
            LinePattern = new HiddenLineAttributeKitLinePatternProperty(kit);
            LineSorting = new HiddenLineAttributeKitLineSortingProperty(kit);
            RenderFaces = new HiddenLineAttributeKitRenderFacesProperty(kit);
            RenderText = new HiddenLineAttributeKitRenderTextProperty(kit);
            SilhouetteCleanup = new HiddenLineAttributeKitSilhouetteCleanupProperty(kit);
            TransparencyCutoff = new HiddenLineAttributeKitTransparencyCutoffProperty(kit);
            Visibility = new HiddenLineAttributeKitVisibilityProperty(kit);
            Weight = new HiddenLineAttributeKitWeightProperty(kit);
        }

        [PropertyOrder(0)]
        public HiddenLineAttributeKitAlgorithmProperty Algorithm { get; set; }

        [PropertyOrder(1)]
        public HiddenLineAttributeKitColorProperty Color { get; set; }

        [PropertyOrder(2)]
        public HiddenLineAttributeKitDimFactorProperty DimFactor { get; set; }

        [PropertyOrder(3)]
        public HiddenLineAttributeKitFaceDisplacementProperty FaceDisplacement { get; set; }

        [PropertyOrder(4)]
        public HiddenLineAttributeKitLinePatternProperty LinePattern { get; set; }

        [PropertyOrder(5)]
        public HiddenLineAttributeKitLineSortingProperty LineSorting { get; set; }

        [PropertyOrder(6)]
        public HiddenLineAttributeKitRenderFacesProperty RenderFaces { get; set; }

        [PropertyOrder(7)]
        public HiddenLineAttributeKitRenderTextProperty RenderText { get; set; }

        [PropertyOrder(8)]
        public HiddenLineAttributeKitSilhouetteCleanupProperty SilhouetteCleanup { get; set; }

        [PropertyOrder(9)]
        public HiddenLineAttributeKitTransparencyCutoffProperty TransparencyCutoff { get; set; }

        [PropertyOrder(10)]
        public HiddenLineAttributeKitVisibilityProperty Visibility { get; set; }

        [PropertyOrder(11)]
        public HiddenLineAttributeKitWeightProperty Weight { get; set; }

        public void Apply()
        {
            key.UnsetHiddenLineAttribute();
            key.SetHiddenLineAttribute(kit);
        }
    }
    #endregion

    #region DrawingAttributeKit
    [ExpandableObject]
    public class DrawingAttributeKitPolygonHandednessProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private HPS.Drawing.Handedness _handedness;

        public DrawingAttributeKitPolygonHandednessProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowPolygonHandedness(out _handedness);
            if (!set)
            {
                _handedness = HPS.Drawing.Handedness.Right;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Handedness", typeof(HPS.Drawing.Handedness), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Handedness": e.Value = _handedness; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Handedness":
                {
                    _handedness = (HPS.Drawing.Handedness)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetPolygonHandedness(_handedness);
            else
                kit.UnsetPolygonHandedness();
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitWorldHandednessProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private HPS.Drawing.Handedness _handedness;

        public DrawingAttributeKitWorldHandednessProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowWorldHandedness(out _handedness);
            if (!set)
            {
                _handedness = HPS.Drawing.Handedness.Right;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Handedness", typeof(HPS.Drawing.Handedness), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Handedness": e.Value = _handedness; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Handedness":
                {
                    _handedness = (HPS.Drawing.Handedness)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetWorldHandedness(_handedness);
            else
                kit.UnsetWorldHandedness();
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitDepthRangeProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private float _near;
        private float _far;

        public DrawingAttributeKitDepthRangeProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDepthRange(out _near, out _far);
            if (!set)
            {
                _near = 0.0f;
                _far = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Near", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Far", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Near": e.Value = _near; break;
                case "Far": e.Value = _far; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Near":
                {
                    _near = (float)e.Value;
                }
                break;
                case "Far":
                {
                    _far = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDepthRange(_near, _far);
            else
                kit.UnsetDepthRange();
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitFaceDisplacementProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private bool _state;
        private int _buckets;

        public DrawingAttributeKitFaceDisplacementProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowFaceDisplacement(out _state, out _buckets);
            if (!set)
            {
                _state = true;
                _buckets = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Buckets", typeof(int), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Buckets": e.Value = _buckets; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Buckets":
                {
                    _buckets = (int)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetFaceDisplacement(_state, _buckets);
            else
                kit.UnsetFaceDisplacement();
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitGeneralDisplacementProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private bool _state;
        private int _buckets;

        public DrawingAttributeKitGeneralDisplacementProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowGeneralDisplacement(out _state, out _buckets);
            if (!set)
            {
                _state = true;
                _buckets = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Buckets", typeof(int), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Buckets": e.Value = _buckets; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Buckets":
                {
                    _buckets = (int)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetGeneralDisplacement(_state, _buckets);
            else
                kit.UnsetGeneralDisplacement();
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitVertexDisplacementProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private bool _state;
        private int _buckets;

        public DrawingAttributeKitVertexDisplacementProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowVertexDisplacement(out _state, out _buckets);
            if (!set)
            {
                _state = true;
                _buckets = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Buckets", typeof(int), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Buckets": e.Value = _buckets; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Buckets":
                {
                    _buckets = (int)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVertexDisplacement(_state, _buckets);
            else
                kit.UnsetVertexDisplacement();
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitVertexDecimationProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private float _zero_to_one;

        public DrawingAttributeKitVertexDecimationProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowVertexDecimation(out _zero_to_one);
            if (!set)
            {
                _zero_to_one = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Zero To One", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Zero To One": e.Value = _zero_to_one; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Zero To One":
                {
                    _zero_to_one = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVertexDecimation(_zero_to_one);
            else
                kit.UnsetVertexDecimation();
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitVertexRandomizationProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private bool _state;

        public DrawingAttributeKitVertexRandomizationProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowVertexRandomization(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVertexRandomization(_state);
            else
                kit.UnsetVertexRandomization();
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitOverlayProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private HPS.Drawing.Overlay _overlay;

        public DrawingAttributeKitOverlayProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowOverlay(out _overlay);
            if (!set)
            {
                _overlay = HPS.Drawing.Overlay.Default;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Overlay", typeof(HPS.Drawing.Overlay), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Overlay": e.Value = _overlay; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Overlay":
                {
                    _overlay = (HPS.Drawing.Overlay)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetOverlay(_overlay);
            else
                kit.UnsetOverlay();
        }
    }

    [ExpandableObject]
    public class DrawingAttributeKitDeferralProperty : NestedProperty
    {
        private HPS.DrawingAttributeKit kit;
        private bool set;
        private int _defer_batch;

        public DrawingAttributeKitDeferralProperty(
            HPS.DrawingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDeferral(out _defer_batch);
            if (!set)
            {
                _defer_batch = 0;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Defer Batch", typeof(int), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Defer Batch": e.Value = _defer_batch; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Defer Batch":
                {
                    _defer_batch = (int)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDeferral(_defer_batch);
            else
                kit.UnsetDeferral();
        }
    }

    [DisplayName("DrawingAttribute")]
    public class DrawingAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.DrawingAttributeKit kit;

        public DrawingAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowDrawingAttribute(out kit);

            PolygonHandedness = new DrawingAttributeKitPolygonHandednessProperty(kit);
            WorldHandedness = new DrawingAttributeKitWorldHandednessProperty(kit);
            DepthRange = new DrawingAttributeKitDepthRangeProperty(kit);
            FaceDisplacement = new DrawingAttributeKitFaceDisplacementProperty(kit);
            GeneralDisplacement = new DrawingAttributeKitGeneralDisplacementProperty(kit);
            VertexDisplacement = new DrawingAttributeKitVertexDisplacementProperty(kit);
            VertexDecimation = new DrawingAttributeKitVertexDecimationProperty(kit);
            VertexRandomization = new DrawingAttributeKitVertexRandomizationProperty(kit);
            OverrideInternalColor = new DrawingAttributeKitOverrideInternalColorProperty(kit);
            Overlay = new DrawingAttributeKitOverlayProperty(kit);
            Deferral = new DrawingAttributeKitDeferralProperty(kit);
            ClipRegion = new DrawingAttributeKitClipRegionProperty(kit);
        }

        [PropertyOrder(0)]
        public DrawingAttributeKitPolygonHandednessProperty PolygonHandedness { get; set; }

        [PropertyOrder(1)]
        public DrawingAttributeKitWorldHandednessProperty WorldHandedness { get; set; }

        [PropertyOrder(2)]
        public DrawingAttributeKitDepthRangeProperty DepthRange { get; set; }

        [PropertyOrder(3)]
        public DrawingAttributeKitFaceDisplacementProperty FaceDisplacement { get; set; }

        [PropertyOrder(4)]
        public DrawingAttributeKitGeneralDisplacementProperty GeneralDisplacement { get; set; }

        [PropertyOrder(5)]
        public DrawingAttributeKitVertexDisplacementProperty VertexDisplacement { get; set; }

        [PropertyOrder(6)]
        public DrawingAttributeKitVertexDecimationProperty VertexDecimation { get; set; }

        [PropertyOrder(7)]
        public DrawingAttributeKitVertexRandomizationProperty VertexRandomization { get; set; }

        [PropertyOrder(8)]
        public DrawingAttributeKitOverrideInternalColorProperty OverrideInternalColor { get; set; }

        [PropertyOrder(9)]
        public DrawingAttributeKitOverlayProperty Overlay { get; set; }

        [PropertyOrder(10)]
        public DrawingAttributeKitDeferralProperty Deferral { get; set; }

        [PropertyOrder(11)]
        public DrawingAttributeKitClipRegionProperty ClipRegion { get; set; }

        public void Apply()
        {
            key.UnsetDrawingAttribute();
            key.SetDrawingAttribute(kit);
        }
    }
    #endregion

    #region SelectabilityKit
    [ExpandableObject]
    public class SelectabilityKitWindowsProperty : NestedProperty
    {
        private HPS.SelectabilityKit kit;
        private bool set;
        private HPS.Selectability.Value _val;

        public SelectabilityKitWindowsProperty(
            HPS.SelectabilityKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowWindows(out _val);
            if (!set)
            {
                _val = HPS.Selectability.Value.On;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Val", typeof(HPS.Selectability.Value), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Val": e.Value = _val; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Val":
                {
                    _val = (HPS.Selectability.Value)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetWindows(_val);
            else
                kit.UnsetWindows();
        }
    }

    [ExpandableObject]
    public class SelectabilityKitEdgesProperty : NestedProperty
    {
        private HPS.SelectabilityKit kit;
        private bool set;
        private HPS.Selectability.Value _val;

        public SelectabilityKitEdgesProperty(
            HPS.SelectabilityKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowEdges(out _val);
            if (!set)
            {
                _val = HPS.Selectability.Value.On;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Val", typeof(HPS.Selectability.Value), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Val": e.Value = _val; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Val":
                {
                    _val = (HPS.Selectability.Value)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetEdges(_val);
            else
                kit.UnsetEdges();
        }
    }

    [ExpandableObject]
    public class SelectabilityKitFacesProperty : NestedProperty
    {
        private HPS.SelectabilityKit kit;
        private bool set;
        private HPS.Selectability.Value _val;

        public SelectabilityKitFacesProperty(
            HPS.SelectabilityKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowFaces(out _val);
            if (!set)
            {
                _val = HPS.Selectability.Value.On;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Val", typeof(HPS.Selectability.Value), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Val": e.Value = _val; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Val":
                {
                    _val = (HPS.Selectability.Value)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetFaces(_val);
            else
                kit.UnsetFaces();
        }
    }

    [ExpandableObject]
    public class SelectabilityKitLightsProperty : NestedProperty
    {
        private HPS.SelectabilityKit kit;
        private bool set;
        private HPS.Selectability.Value _val;

        public SelectabilityKitLightsProperty(
            HPS.SelectabilityKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowLights(out _val);
            if (!set)
            {
                _val = HPS.Selectability.Value.On;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Val", typeof(HPS.Selectability.Value), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Val": e.Value = _val; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Val":
                {
                    _val = (HPS.Selectability.Value)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetLights(_val);
            else
                kit.UnsetLights();
        }
    }

    [ExpandableObject]
    public class SelectabilityKitLinesProperty : NestedProperty
    {
        private HPS.SelectabilityKit kit;
        private bool set;
        private HPS.Selectability.Value _val;

        public SelectabilityKitLinesProperty(
            HPS.SelectabilityKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowLines(out _val);
            if (!set)
            {
                _val = HPS.Selectability.Value.On;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Val", typeof(HPS.Selectability.Value), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Val": e.Value = _val; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Val":
                {
                    _val = (HPS.Selectability.Value)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetLines(_val);
            else
                kit.UnsetLines();
        }
    }

    [ExpandableObject]
    public class SelectabilityKitMarkersProperty : NestedProperty
    {
        private HPS.SelectabilityKit kit;
        private bool set;
        private HPS.Selectability.Value _val;

        public SelectabilityKitMarkersProperty(
            HPS.SelectabilityKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowMarkers(out _val);
            if (!set)
            {
                _val = HPS.Selectability.Value.On;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Val", typeof(HPS.Selectability.Value), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Val": e.Value = _val; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Val":
                {
                    _val = (HPS.Selectability.Value)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetMarkers(_val);
            else
                kit.UnsetMarkers();
        }
    }

    [ExpandableObject]
    public class SelectabilityKitVerticesProperty : NestedProperty
    {
        private HPS.SelectabilityKit kit;
        private bool set;
        private HPS.Selectability.Value _val;

        public SelectabilityKitVerticesProperty(
            HPS.SelectabilityKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowVertices(out _val);
            if (!set)
            {
                _val = HPS.Selectability.Value.On;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Val", typeof(HPS.Selectability.Value), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Val": e.Value = _val; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Val":
                {
                    _val = (HPS.Selectability.Value)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVertices(_val);
            else
                kit.UnsetVertices();
        }
    }

    [ExpandableObject]
    public class SelectabilityKitTextProperty : NestedProperty
    {
        private HPS.SelectabilityKit kit;
        private bool set;
        private HPS.Selectability.Value _val;

        public SelectabilityKitTextProperty(
            HPS.SelectabilityKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowText(out _val);
            if (!set)
            {
                _val = HPS.Selectability.Value.On;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Val", typeof(HPS.Selectability.Value), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Val": e.Value = _val; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Val":
                {
                    _val = (HPS.Selectability.Value)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetText(_val);
            else
                kit.UnsetText();
        }
    }

    [DisplayName("Selectability")]
    public class SelectabilityKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.SelectabilityKit kit;

        public SelectabilityKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowSelectability(out kit);

            Windows = new SelectabilityKitWindowsProperty(kit);
            Edges = new SelectabilityKitEdgesProperty(kit);
            Faces = new SelectabilityKitFacesProperty(kit);
            Lights = new SelectabilityKitLightsProperty(kit);
            Lines = new SelectabilityKitLinesProperty(kit);
            Markers = new SelectabilityKitMarkersProperty(kit);
            Vertices = new SelectabilityKitVerticesProperty(kit);
            Text = new SelectabilityKitTextProperty(kit);
        }

        [PropertyOrder(0)]
        public SelectabilityKitWindowsProperty Windows { get; set; }

        [PropertyOrder(1)]
        public SelectabilityKitEdgesProperty Edges { get; set; }

        [PropertyOrder(2)]
        public SelectabilityKitFacesProperty Faces { get; set; }

        [PropertyOrder(3)]
        public SelectabilityKitLightsProperty Lights { get; set; }

        [PropertyOrder(4)]
        public SelectabilityKitLinesProperty Lines { get; set; }

        [PropertyOrder(5)]
        public SelectabilityKitMarkersProperty Markers { get; set; }

        [PropertyOrder(6)]
        public SelectabilityKitVerticesProperty Vertices { get; set; }

        [PropertyOrder(7)]
        public SelectabilityKitTextProperty Text { get; set; }

        public void Apply()
        {
            key.UnsetSelectability();
            key.SetSelectability(kit);
        }
    }
    #endregion

    #region MarkerAttributeKit
    [ExpandableObject]
    public class MarkerAttributeKitSymbolProperty : NestedProperty
    {
        private HPS.MarkerAttributeKit kit;
        private bool set;
        private string _glyph_name;

        public MarkerAttributeKitSymbolProperty(
            HPS.MarkerAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSymbol(out _glyph_name);
            if (!set)
            {
                _glyph_name = "glyph_name";
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Glyph Name", typeof(string), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Glyph Name": e.Value = _glyph_name; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Glyph Name":
                {
                    _glyph_name = (string)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSymbol(_glyph_name);
            else
                kit.UnsetSymbol();
        }
    }

    [ExpandableObject]
    public class MarkerAttributeKitSizeProperty : NestedProperty
    {
        private HPS.MarkerAttributeKit kit;
        private bool set;
        private float _size;
        private HPS.Marker.SizeUnits _units;

        public MarkerAttributeKitSizeProperty(
            HPS.MarkerAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSize(out _size, out _units);
            if (!set)
            {
                _size = 0.0f;
                _units = HPS.Marker.SizeUnits.ScaleFactor;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Size", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Units", typeof(HPS.Marker.SizeUnits), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Size": e.Value = _size; break;
                case "Units": e.Value = _units; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Size":
                {
                    _size = (float)e.Value;
                }
                break;
                case "Units":
                {
                    _units = (HPS.Marker.SizeUnits)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSize(_size, _units);
            else
                kit.UnsetSize();
        }
    }

    [ExpandableObject]
    public class MarkerAttributeKitDrawingPreferenceProperty : NestedProperty
    {
        private HPS.MarkerAttributeKit kit;
        private bool set;
        private HPS.Marker.DrawingPreference _preference;

        public MarkerAttributeKitDrawingPreferenceProperty(
            HPS.MarkerAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDrawingPreference(out _preference);
            if (!set)
            {
                _preference = HPS.Marker.DrawingPreference.Fastest;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Preference", typeof(HPS.Marker.DrawingPreference), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Preference": e.Value = _preference; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Preference":
                {
                    _preference = (HPS.Marker.DrawingPreference)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDrawingPreference(_preference);
            else
                kit.UnsetDrawingPreference();
        }
    }

    [ExpandableObject]
    public class MarkerAttributeKitGlyphRotationProperty : NestedProperty
    {
        private HPS.MarkerAttributeKit kit;
        private bool set;
        private float _rotation;

        public MarkerAttributeKitGlyphRotationProperty(
            HPS.MarkerAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowGlyphRotation(out _rotation);
            if (!set)
            {
                _rotation = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Rotation", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Rotation": e.Value = _rotation; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Rotation":
                {
                    _rotation = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetGlyphRotation(_rotation);
            else
                kit.UnsetGlyphRotation();
        }
    }

    [DisplayName("MarkerAttribute")]
    public class MarkerAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.MarkerAttributeKit kit;

        public MarkerAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowMarkerAttribute(out kit);

            Symbol = new MarkerAttributeKitSymbolProperty(kit);
            Size = new MarkerAttributeKitSizeProperty(kit);
            DrawingPreference = new MarkerAttributeKitDrawingPreferenceProperty(kit);
            GlyphRotation = new MarkerAttributeKitGlyphRotationProperty(kit);
        }

        [PropertyOrder(0)]
        public MarkerAttributeKitSymbolProperty Symbol { get; set; }

        [PropertyOrder(1)]
        public MarkerAttributeKitSizeProperty Size { get; set; }

        [PropertyOrder(2)]
        public MarkerAttributeKitDrawingPreferenceProperty DrawingPreference { get; set; }

        [PropertyOrder(3)]
        public MarkerAttributeKitGlyphRotationProperty GlyphRotation { get; set; }

        public void Apply()
        {
            key.UnsetMarkerAttribute();
            key.SetMarkerAttribute(kit);
        }
    }
    #endregion

    #region LightingAttributeKit
    [ExpandableObject]
    public class LightingAttributeKitInterpolationAlgorithmProperty : NestedProperty
    {
        private HPS.LightingAttributeKit kit;
        private bool set;
        private HPS.Lighting.InterpolationAlgorithm _interpolation;

        public LightingAttributeKitInterpolationAlgorithmProperty(
            HPS.LightingAttributeKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowInterpolationAlgorithm(out _interpolation);
            if (!set)
            {
                _interpolation = HPS.Lighting.InterpolationAlgorithm.Phong;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Interpolation", typeof(HPS.Lighting.InterpolationAlgorithm), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Interpolation": e.Value = _interpolation; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Interpolation":
                {
                    _interpolation = (HPS.Lighting.InterpolationAlgorithm)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetInterpolationAlgorithm(_interpolation);
            else
                kit.UnsetInterpolationAlgorithm();
        }
    }

    [DisplayName("LightingAttribute")]
    public class LightingAttributeKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.LightingAttributeKit kit;

        public LightingAttributeKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowLightingAttribute(out kit);

            InterpolationAlgorithm = new LightingAttributeKitInterpolationAlgorithmProperty(kit);
        }

        [PropertyOrder(0)]
        public LightingAttributeKitInterpolationAlgorithmProperty InterpolationAlgorithm { get; set; }

        public void Apply()
        {
            key.UnsetLightingAttribute();
            key.SetLightingAttribute(kit);
        }
    }
    #endregion

    #region VisualEffectsKit
    [ExpandableObject]
    public class VisualEffectsKitPostProcessEffectsEnabledProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;

        public VisualEffectsKitPostProcessEffectsEnabledProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowPostProcessEffectsEnabled(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetPostProcessEffectsEnabled(_state);
            else
                kit.UnsetPostProcessEffectsEnabled();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitAmbientOcclusionEnabledProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;

        public VisualEffectsKitAmbientOcclusionEnabledProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowAmbientOcclusionEnabled(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetAmbientOcclusionEnabled(_state);
            else
                kit.UnsetAmbientOcclusionEnabled();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitSilhouetteEdgesEnabledProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;

        public VisualEffectsKitSilhouetteEdgesEnabledProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSilhouetteEdgesEnabled(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSilhouetteEdgesEnabled(_state);
            else
                kit.UnsetSilhouetteEdgesEnabled();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitDepthOfFieldEnabledProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;

        public VisualEffectsKitDepthOfFieldEnabledProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowDepthOfFieldEnabled(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetDepthOfFieldEnabled(_state);
            else
                kit.UnsetDepthOfFieldEnabled();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitBloomEnabledProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;

        public VisualEffectsKitBloomEnabledProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowBloomEnabled(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetBloomEnabled(_state);
            else
                kit.UnsetBloomEnabled();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitEyeDomeLightingEnabledProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;

        public VisualEffectsKitEyeDomeLightingEnabledProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowEyeDomeLightingEnabled(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetEyeDomeLightingEnabled(_state);
            else
                kit.UnsetEyeDomeLightingEnabled();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitAntiAliasingProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;

        public VisualEffectsKitAntiAliasingProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowAntiAliasing(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetAntiAliasing(_state);
            else
                kit.UnsetAntiAliasing();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitTextAntiAliasingProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;

        public VisualEffectsKitTextAntiAliasingProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowTextAntiAliasing(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetTextAntiAliasing(_state);
            else
                kit.UnsetTextAntiAliasing();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitShadowMapsProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;
        private uint _samples;
        private uint _resolution;
        private bool _view_dependent;
        private bool _jitter;

        public VisualEffectsKitShadowMapsProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowShadowMaps(out _state, out _samples, out _resolution, out _view_dependent, out _jitter);
            if (!set)
            {
                _state = true;
                _samples = 0;
                _resolution = 0;
                _view_dependent = true;
                _jitter = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Samples", typeof(uint), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Resolution", typeof(uint), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("View Dependent", typeof(bool), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Jitter", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Samples": e.Value = _samples; break;
                case "Resolution": e.Value = _resolution; break;
                case "View Dependent": e.Value = _view_dependent; break;
                case "Jitter": e.Value = _jitter; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Samples":
                {
                    _samples = (uint)e.Value;
                }
                break;
                case "Resolution":
                {
                    _resolution = (uint)e.Value;
                }
                break;
                case "View Dependent":
                {
                    _view_dependent = (bool)e.Value;
                }
                break;
                case "Jitter":
                {
                    _jitter = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetShadowMaps(_state, _samples, _resolution, _view_dependent, _jitter);
            else
                kit.UnsetShadowMaps();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitSimpleShadowProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;
        private uint _resolution;
        private uint _blurring;
        private bool _ignore_transparency;

        public VisualEffectsKitSimpleShadowProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSimpleShadow(out _state, out _resolution, out _blurring, out _ignore_transparency);
            if (!set)
            {
                _state = true;
                _resolution = 0;
                _blurring = 0;
                _ignore_transparency = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Resolution", typeof(uint), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Blurring", typeof(uint), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Ignore Transparency", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Resolution": e.Value = _resolution; break;
                case "Blurring": e.Value = _blurring; break;
                case "Ignore Transparency": e.Value = _ignore_transparency; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Resolution":
                {
                    _resolution = (uint)e.Value;
                }
                break;
                case "Blurring":
                {
                    _blurring = (uint)e.Value;
                }
                break;
                case "Ignore Transparency":
                {
                    _ignore_transparency = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSimpleShadow(_state, _resolution, _blurring, _ignore_transparency);
            else
                kit.UnsetSimpleShadow();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitSimpleShadowPlaneProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private HPS.Plane _projected_onto;
        private PlaneProperty _projected_ontoProperty;

        public VisualEffectsKitSimpleShadowPlaneProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSimpleShadowPlane(out _projected_onto);
            if (!set)
            {
                _projected_onto = new HPS.Plane(0, 0, 1, 0);
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            _projected_ontoProperty = new PlaneProperty(_projected_onto, Update_projected_onto);
            Properties.Add(new PropertySpec("Projected Onto", typeof(PlaneProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Projected Onto": e.Value = _projected_ontoProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Projected Onto":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSimpleShadowPlane(_projected_onto);
            else
                kit.UnsetSimpleShadowPlane();
        }

        private void Update_projected_onto(
            HPS.Plane _projected_onto)
        {
            this._projected_onto = _projected_onto;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitSimpleShadowLightDirectionProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private HPS.Vector _direction;
        private VectorProperty _directionProperty;

        public VisualEffectsKitSimpleShadowLightDirectionProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSimpleShadowLightDirection(out _direction);
            if (!set)
            {
                _direction = HPS.Vector.Unit();
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            _directionProperty = new VectorProperty(_direction, Update_direction);
            Properties.Add(new PropertySpec("Direction", typeof(VectorProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Direction": e.Value = _directionProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Direction":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSimpleShadowLightDirection(_direction);
            else
                kit.UnsetSimpleShadowLightDirection();
        }

        private void Update_direction(
            HPS.Vector _direction)
        {
            this._direction = _direction;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitSimpleShadowColorProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private HPS.RGBAColor _color;
        private RGBAColorProperty _colorProperty;

        public VisualEffectsKitSimpleShadowColorProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSimpleShadowColor(out _color);
            if (!set)
            {
                _color = HPS.RGBAColor.Black();
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            _colorProperty = new RGBAColorProperty(_color, Update_color);
            Properties.Add(new PropertySpec("Color", typeof(RGBAColorProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Color": e.Value = _colorProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Color":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSimpleShadowColor(_color);
            else
                kit.UnsetSimpleShadowColor();
        }

        private void Update_color(
            HPS.RGBAColor _color)
        {
            this._color = _color;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitSimpleReflectionProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private bool _state;
        private float _opacity;
        private uint _blurring;
        private bool _fading;
        private float _attenuation_near_distance;
        private float _attenuation_far_distance;

        public VisualEffectsKitSimpleReflectionProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSimpleReflection(out _state, out _opacity, out _blurring, out _fading, out _attenuation_near_distance, out _attenuation_far_distance);
            if (!set)
            {
                _state = true;
                _opacity = 0.0f;
                _blurring = 0;
                _fading = true;
                _attenuation_near_distance = 0.0f;
                _attenuation_far_distance = 0.0f;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Opacity", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Blurring", typeof(uint), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Fading", typeof(bool), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Attenuation Near Distance", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Attenuation Far Distance", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
                case "Opacity": e.Value = _opacity; break;
                case "Blurring": e.Value = _blurring; break;
                case "Fading": e.Value = _fading; break;
                case "Attenuation Near Distance": e.Value = _attenuation_near_distance; break;
                case "Attenuation Far Distance": e.Value = _attenuation_far_distance; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Opacity":
                {
                    _opacity = (float)e.Value;
                }
                break;
                case "Blurring":
                {
                    _blurring = (uint)e.Value;
                }
                break;
                case "Fading":
                {
                    _fading = (bool)e.Value;
                }
                break;
                case "Attenuation Near Distance":
                {
                    _attenuation_near_distance = (float)e.Value;
                }
                break;
                case "Attenuation Far Distance":
                {
                    _attenuation_far_distance = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSimpleReflection(_state, _opacity, _blurring, _fading, _attenuation_near_distance, _attenuation_far_distance);
            else
                kit.UnsetSimpleReflection();
        }
    }

    [ExpandableObject]
    public class VisualEffectsKitSimpleReflectionPlaneProperty : NestedProperty
    {
        private HPS.VisualEffectsKit kit;
        private bool set;
        private HPS.Plane _projected_onto;
        private PlaneProperty _projected_ontoProperty;

        public VisualEffectsKitSimpleReflectionPlaneProperty(
            HPS.VisualEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowSimpleReflectionPlane(out _projected_onto);
            if (!set)
            {
                _projected_onto = new HPS.Plane(0, 0, 1, 0);
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            _projected_ontoProperty = new PlaneProperty(_projected_onto, Update_projected_onto);
            Properties.Add(new PropertySpec("Projected Onto", typeof(PlaneProperty), null, expandable: true, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Projected Onto": e.Value = _projected_ontoProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Projected Onto":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetSimpleReflectionPlane(_projected_onto);
            else
                kit.UnsetSimpleReflectionPlane();
        }

        private void Update_projected_onto(
            HPS.Plane _projected_onto)
        {
            this._projected_onto = _projected_onto;
            UpdateKit();
        }
    }

    [DisplayName("VisualEffects")]
    public class VisualEffectsKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.VisualEffectsKit kit;

        public VisualEffectsKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowVisualEffects(out kit);

            PostProcessEffectsEnabled = new VisualEffectsKitPostProcessEffectsEnabledProperty(kit);
            AmbientOcclusionEnabled = new VisualEffectsKitAmbientOcclusionEnabledProperty(kit);
            SilhouetteEdgesEnabled = new VisualEffectsKitSilhouetteEdgesEnabledProperty(kit);
            DepthOfFieldEnabled = new VisualEffectsKitDepthOfFieldEnabledProperty(kit);
            BloomEnabled = new VisualEffectsKitBloomEnabledProperty(kit);
            EyeDomeLightingEnabled = new VisualEffectsKitEyeDomeLightingEnabledProperty(kit);
            EyeDomeLightingBackColor = new VisualEffectsKitEyeDomeLightingBackColorProperty(kit);
            AntiAliasing = new VisualEffectsKitAntiAliasingProperty(kit);
            TextAntiAliasing = new VisualEffectsKitTextAntiAliasingProperty(kit);
            ShadowMaps = new VisualEffectsKitShadowMapsProperty(kit);
            SimpleShadow = new VisualEffectsKitSimpleShadowProperty(kit);
            SimpleShadowPlane = new VisualEffectsKitSimpleShadowPlaneProperty(kit);
            SimpleShadowLightDirection = new VisualEffectsKitSimpleShadowLightDirectionProperty(kit);
            SimpleShadowColor = new VisualEffectsKitSimpleShadowColorProperty(kit);
            SimpleReflection = new VisualEffectsKitSimpleReflectionProperty(kit);
            SimpleReflectionPlane = new VisualEffectsKitSimpleReflectionPlaneProperty(kit);
            SimpleReflectionVisibility = new VisualEffectsKitSimpleReflectionVisibilityProperty(kit);
        }

        [PropertyOrder(0)]
        public VisualEffectsKitPostProcessEffectsEnabledProperty PostProcessEffectsEnabled { get; set; }

        [PropertyOrder(1)]
        public VisualEffectsKitAmbientOcclusionEnabledProperty AmbientOcclusionEnabled { get; set; }

        [PropertyOrder(2)]
        public VisualEffectsKitSilhouetteEdgesEnabledProperty SilhouetteEdgesEnabled { get; set; }

        [PropertyOrder(3)]
        public VisualEffectsKitDepthOfFieldEnabledProperty DepthOfFieldEnabled { get; set; }

        [PropertyOrder(4)]
        public VisualEffectsKitBloomEnabledProperty BloomEnabled { get; set; }

        [PropertyOrder(5)]
        public VisualEffectsKitEyeDomeLightingEnabledProperty EyeDomeLightingEnabled { get; set; }

        [PropertyOrder(6)]
        public VisualEffectsKitEyeDomeLightingBackColorProperty EyeDomeLightingBackColor { get; set; }

        [PropertyOrder(7)]
        public VisualEffectsKitAntiAliasingProperty AntiAliasing { get; set; }

        [PropertyOrder(8)]
        public VisualEffectsKitTextAntiAliasingProperty TextAntiAliasing { get; set; }

        [PropertyOrder(9)]
        public VisualEffectsKitShadowMapsProperty ShadowMaps { get; set; }

        [PropertyOrder(10)]
        public VisualEffectsKitSimpleShadowProperty SimpleShadow { get; set; }

        [PropertyOrder(11)]
        public VisualEffectsKitSimpleShadowPlaneProperty SimpleShadowPlane { get; set; }

        [PropertyOrder(12)]
        public VisualEffectsKitSimpleShadowLightDirectionProperty SimpleShadowLightDirection { get; set; }

        [PropertyOrder(13)]
        public VisualEffectsKitSimpleShadowColorProperty SimpleShadowColor { get; set; }

        [PropertyOrder(14)]
        public VisualEffectsKitSimpleReflectionProperty SimpleReflection { get; set; }

        [PropertyOrder(15)]
        public VisualEffectsKitSimpleReflectionPlaneProperty SimpleReflectionPlane { get; set; }

        [PropertyOrder(16)]
        public VisualEffectsKitSimpleReflectionVisibilityProperty SimpleReflectionVisibility { get; set; }

        public void Apply()
        {
            key.UnsetVisualEffects();
            key.SetVisualEffects(kit);
        }
    }
    #endregion

    #region PostProcessEffectsKit
    [ExpandableObject]
    public class PostProcessEffectsKitAmbientOcclusionProperty : NestedProperty
    {
        private HPS.PostProcessEffectsKit kit;
        private bool _state;
        private float _strength;
        private HPS.PostProcessEffects.AmbientOcclusion.Quality _quality;
        private float _radius;
        private float _sharpness;

        public PostProcessEffectsKitAmbientOcclusionProperty(
            HPS.PostProcessEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowAmbientOcclusion(out _state, out _strength, out _quality, out _radius, out _sharpness);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Strength", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Quality", typeof(HPS.PostProcessEffects.AmbientOcclusion.Quality), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Radius", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Sharpness", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
                case "Strength": e.Value = _strength; break;
                case "Quality": e.Value = _quality; break;
                case "Radius": e.Value = _radius; break;
                case "Sharpness": e.Value = _sharpness; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Strength":
                {
                    _strength = (float)e.Value;
                }
                break;
                case "Quality":
                {
                    _quality = (HPS.PostProcessEffects.AmbientOcclusion.Quality)e.Value;
                }
                break;
                case "Radius":
                {
                    _radius = (float)e.Value;
                }
                break;
                case "Sharpness":
                {
                    _sharpness = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetAmbientOcclusion(_state, _strength, _quality, _radius, _sharpness);
        }
    }

    [ExpandableObject]
    public class PostProcessEffectsKitBloomProperty : NestedProperty
    {
        private HPS.PostProcessEffectsKit kit;
        private bool _state;
        private float _strength;
        private uint _blur;
        private HPS.PostProcessEffects.Bloom.Shape _shape;

        public PostProcessEffectsKitBloomProperty(
            HPS.PostProcessEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowBloom(out _state, out _strength, out _blur, out _shape);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Strength", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Blur", typeof(uint), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Shape", typeof(HPS.PostProcessEffects.Bloom.Shape), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
                case "Strength": e.Value = _strength; break;
                case "Blur": e.Value = _blur; break;
                case "Shape": e.Value = _shape; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Strength":
                {
                    _strength = (float)e.Value;
                }
                break;
                case "Blur":
                {
                    _blur = (uint)e.Value;
                }
                break;
                case "Shape":
                {
                    _shape = (HPS.PostProcessEffects.Bloom.Shape)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetBloom(_state, _strength, _blur, _shape);
        }
    }

    [ExpandableObject]
    public class PostProcessEffectsKitDepthOfFieldProperty : NestedProperty
    {
        private HPS.PostProcessEffectsKit kit;
        private bool _state;
        private float _strength;
        private float _near_distance;
        private float _far_distance;

        public PostProcessEffectsKitDepthOfFieldProperty(
            HPS.PostProcessEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowDepthOfField(out _state, out _strength, out _near_distance, out _far_distance);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Strength", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Near Distance", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Far Distance", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
                case "Strength": e.Value = _strength; break;
                case "Near Distance": e.Value = _near_distance; break;
                case "Far Distance": e.Value = _far_distance; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Strength":
                {
                    _strength = (float)e.Value;
                }
                break;
                case "Near Distance":
                {
                    _near_distance = (float)e.Value;
                }
                break;
                case "Far Distance":
                {
                    _far_distance = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetDepthOfField(_state, _strength, _near_distance, _far_distance);
        }
    }

    [ExpandableObject]
    public class PostProcessEffectsKitSilhouetteEdgesProperty : NestedProperty
    {
        private HPS.PostProcessEffectsKit kit;
        private bool _state;
        private float _tolerance;
        private bool _heavy_exterior;

        public PostProcessEffectsKitSilhouetteEdgesProperty(
            HPS.PostProcessEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowSilhouetteEdges(out _state, out _tolerance, out _heavy_exterior);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Tolerance", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Heavy Exterior", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
                case "Tolerance": e.Value = _tolerance; break;
                case "Heavy Exterior": e.Value = _heavy_exterior; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Tolerance":
                {
                    _tolerance = (float)e.Value;
                }
                break;
                case "Heavy Exterior":
                {
                    _heavy_exterior = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetSilhouetteEdges(_state, _tolerance, _heavy_exterior);
        }
    }

    [ExpandableObject]
    public class PostProcessEffectsKitEyeDomeLightingProperty : NestedProperty
    {
        private HPS.PostProcessEffectsKit kit;
        private bool _state;
        private float _exponent;
        private float _tolerance;
        private float _strength;

        public PostProcessEffectsKitEyeDomeLightingProperty(
            HPS.PostProcessEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowEyeDomeLighting(out _state, out _exponent, out _tolerance, out _strength);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Exponent", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Tolerance", typeof(float), null, expandable: false, triggersRefresh: false));
            Properties.Add(new PropertySpec("Strength", typeof(float), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.EnableValidProperties(Properties, _state);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State": e.Value = _state; break;
                case "Exponent": e.Value = _exponent; break;
                case "Tolerance": e.Value = _tolerance; break;
                case "Strength": e.Value = _strength; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "State":
                {
                    _state = (bool)e.Value;
                    PropertyUtilities.EnableValidProperties(Properties, _state);
                }
                break;
                case "Exponent":
                {
                    _exponent = (float)e.Value;
                }
                break;
                case "Tolerance":
                {
                    _tolerance = (float)e.Value;
                }
                break;
                case "Strength":
                {
                    _strength = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetEyeDomeLighting(_state, _exponent, _tolerance, _strength);
        }
    }

    [ExpandableObject]
    public class PostProcessEffectsKitWorldScaleProperty : NestedProperty
    {
        private HPS.PostProcessEffectsKit kit;
        private float _scale;

        public PostProcessEffectsKitWorldScaleProperty(
            HPS.PostProcessEffectsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowWorldScale(out _scale);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Scale", typeof(float), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Scale": e.Value = _scale; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Scale":
                {
                    _scale = (float)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetWorldScale(_scale);
        }
    }

    [DisplayName("PostProcessEffects")]
    public class PostProcessEffectsKitProperty : IRootProperty
    {
        private HPS.WindowKey key;
        private HPS.PostProcessEffectsKit kit;

        public PostProcessEffectsKitProperty(
            HPS.WindowKey key)
        {
            this.key = key;
            this.key.ShowPostProcessEffects(out kit);

            AmbientOcclusion = new PostProcessEffectsKitAmbientOcclusionProperty(kit);
            Bloom = new PostProcessEffectsKitBloomProperty(kit);
            DepthOfField = new PostProcessEffectsKitDepthOfFieldProperty(kit);
            SilhouetteEdges = new PostProcessEffectsKitSilhouetteEdgesProperty(kit);
            EyeDomeLighting = new PostProcessEffectsKitEyeDomeLightingProperty(kit);
            WorldScale = new PostProcessEffectsKitWorldScaleProperty(kit);
        }

        [PropertyOrder(0)]
        public PostProcessEffectsKitAmbientOcclusionProperty AmbientOcclusion { get; set; }

        [PropertyOrder(1)]
        public PostProcessEffectsKitBloomProperty Bloom { get; set; }

        [PropertyOrder(2)]
        public PostProcessEffectsKitDepthOfFieldProperty DepthOfField { get; set; }

        [PropertyOrder(3)]
        public PostProcessEffectsKitSilhouetteEdgesProperty SilhouetteEdges { get; set; }

        [PropertyOrder(4)]
        public PostProcessEffectsKitEyeDomeLightingProperty EyeDomeLighting { get; set; }

        [PropertyOrder(5)]
        public PostProcessEffectsKitWorldScaleProperty WorldScale { get; set; }

        public void Apply()
        {
            key.SetPostProcessEffects(kit);
        }
    }
    #endregion

    #region DebuggingKit
    [ExpandableObject]
    public class DebuggingKitResourceMonitorProperty : NestedProperty
    {
        private HPS.DebuggingKit kit;
        private bool _display;

        public DebuggingKitResourceMonitorProperty(
            HPS.DebuggingKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowResourceMonitor(out _display);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Display", typeof(bool), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Display": e.Value = _display; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Display":
                {
                    _display = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetResourceMonitor(_display);
        }
    }

    [DisplayName("Debugging")]
    public class DebuggingKitProperty : IRootProperty
    {
        private HPS.WindowKey key;
        private HPS.DebuggingKit kit;

        public DebuggingKitProperty(
            HPS.WindowKey key)
        {
            this.key = key;
            this.key.ShowDebugging(out kit);

            ResourceMonitor = new DebuggingKitResourceMonitorProperty(kit);
        }

        [PropertyOrder(0)]
        public DebuggingKitResourceMonitorProperty ResourceMonitor { get; set; }

        public void Apply()
        {
            key.SetDebugging(kit);
        }
    }
    #endregion

    #region ContourLineKit
    [ExpandableObject]
    public class ContourLineKitVisibilityProperty : NestedProperty
    {
        private HPS.ContourLineKit kit;
        private bool set;
        private bool _state;

        public ContourLineKitVisibilityProperty(
            HPS.ContourLineKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowVisibility(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVisibility(_state);
            else
                kit.UnsetVisibility();
        }
    }

    [ExpandableObject]
    public class ContourLineKitLightingProperty : NestedProperty
    {
        private HPS.ContourLineKit kit;
        private bool set;
        private bool _state;

        public ContourLineKitLightingProperty(
            HPS.ContourLineKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowLighting(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetLighting(_state);
            else
                kit.UnsetLighting();
        }
    }

    [DisplayName("ContourLine")]
    public class ContourLineKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.ContourLineKit kit;

        public ContourLineKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowContourLine(out kit);

            Visibility = new ContourLineKitVisibilityProperty(kit);
            Positions = new ContourLineKitPositionsProperty(kit);
            Colors = new ContourLineKitColorsProperty(kit);
            Patterns = new ContourLineKitPatternsProperty(kit);
            Weights = new ContourLineKitWeightsProperty(kit);
            Lighting = new ContourLineKitLightingProperty(kit);
        }

        [PropertyOrder(0)]
        public ContourLineKitVisibilityProperty Visibility { get; set; }

        [PropertyOrder(1)]
        public ContourLineKitPositionsProperty Positions { get; set; }

        [PropertyOrder(2)]
        public ContourLineKitColorsProperty Colors { get; set; }

        [PropertyOrder(3)]
        public ContourLineKitPatternsProperty Patterns { get; set; }

        [PropertyOrder(4)]
        public ContourLineKitWeightsProperty Weights { get; set; }

        [PropertyOrder(5)]
        public ContourLineKitLightingProperty Lighting { get; set; }

        public void Apply()
        {
            key.UnsetContourLine();
            key.SetContourLine(kit);
        }
    }
    #endregion

    #region BoundingKit
    [ExpandableObject]
    public class BoundingKitExclusionProperty : NestedProperty
    {
        private HPS.BoundingKit kit;
        private bool set;
        private bool _exclusion;

        public BoundingKitExclusionProperty(
            HPS.BoundingKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowExclusion(out _exclusion);
            if (!set)
            {
                _exclusion = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("Exclusion", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "Exclusion": e.Value = _exclusion; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "Exclusion":
                {
                    _exclusion = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetExclusion(_exclusion);
            else
                kit.UnsetExclusion();
        }
    }

    [DisplayName("Bounding")]
    public class BoundingKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.BoundingKit kit;

        public BoundingKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowBounding(out kit);

            Volume = new BoundingKitVolumeProperty(kit);
            Exclusion = new BoundingKitExclusionProperty(kit);
        }

        [PropertyOrder(0)]
        public BoundingKitVolumeProperty Volume { get; set; }

        [PropertyOrder(1)]
        public BoundingKitExclusionProperty Exclusion { get; set; }

        public void Apply()
        {
            key.UnsetBounding();
            key.SetBounding(kit);
        }
    }
    #endregion

    #region TransformMaskKit
    [ExpandableObject]
    public class TransformMaskKitCameraRotationProperty : NestedProperty
    {
        private HPS.TransformMaskKit kit;
        private bool set;
        private bool _state;

        public TransformMaskKitCameraRotationProperty(
            HPS.TransformMaskKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowCameraRotation(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCameraRotation(_state);
            else
                kit.UnsetCameraRotation();
        }
    }

    [ExpandableObject]
    public class TransformMaskKitCameraScaleProperty : NestedProperty
    {
        private HPS.TransformMaskKit kit;
        private bool set;
        private bool _state;

        public TransformMaskKitCameraScaleProperty(
            HPS.TransformMaskKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowCameraScale(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCameraScale(_state);
            else
                kit.UnsetCameraScale();
        }
    }

    [ExpandableObject]
    public class TransformMaskKitCameraTranslationProperty : NestedProperty
    {
        private HPS.TransformMaskKit kit;
        private bool set;
        private bool _state;

        public TransformMaskKitCameraTranslationProperty(
            HPS.TransformMaskKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowCameraTranslation(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCameraTranslation(_state);
            else
                kit.UnsetCameraTranslation();
        }
    }

    [ExpandableObject]
    public class TransformMaskKitCameraPerspectiveScaleProperty : NestedProperty
    {
        private HPS.TransformMaskKit kit;
        private bool set;
        private bool _state;

        public TransformMaskKitCameraPerspectiveScaleProperty(
            HPS.TransformMaskKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowCameraPerspectiveScale(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCameraPerspectiveScale(_state);
            else
                kit.UnsetCameraPerspectiveScale();
        }
    }

    [ExpandableObject]
    public class TransformMaskKitCameraProjectionProperty : NestedProperty
    {
        private HPS.TransformMaskKit kit;
        private bool set;
        private bool _state;

        public TransformMaskKitCameraProjectionProperty(
            HPS.TransformMaskKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowCameraProjection(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCameraProjection(_state);
            else
                kit.UnsetCameraProjection();
        }
    }

    [ExpandableObject]
    public class TransformMaskKitCameraOffsetProperty : NestedProperty
    {
        private HPS.TransformMaskKit kit;
        private bool set;
        private bool _state;

        public TransformMaskKitCameraOffsetProperty(
            HPS.TransformMaskKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowCameraOffset(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCameraOffset(_state);
            else
                kit.UnsetCameraOffset();
        }
    }

    [ExpandableObject]
    public class TransformMaskKitCameraNearLimitProperty : NestedProperty
    {
        private HPS.TransformMaskKit kit;
        private bool set;
        private bool _state;

        public TransformMaskKitCameraNearLimitProperty(
            HPS.TransformMaskKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowCameraNearLimit(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetCameraNearLimit(_state);
            else
                kit.UnsetCameraNearLimit();
        }
    }

    [ExpandableObject]
    public class TransformMaskKitModellingMatrixRotationProperty : NestedProperty
    {
        private HPS.TransformMaskKit kit;
        private bool set;
        private bool _state;

        public TransformMaskKitModellingMatrixRotationProperty(
            HPS.TransformMaskKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowModellingMatrixRotation(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetModellingMatrixRotation(_state);
            else
                kit.UnsetModellingMatrixRotation();
        }
    }

    [ExpandableObject]
    public class TransformMaskKitModellingMatrixScaleProperty : NestedProperty
    {
        private HPS.TransformMaskKit kit;
        private bool set;
        private bool _state;

        public TransformMaskKitModellingMatrixScaleProperty(
            HPS.TransformMaskKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowModellingMatrixScale(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetModellingMatrixScale(_state);
            else
                kit.UnsetModellingMatrixScale();
        }
    }

    [ExpandableObject]
    public class TransformMaskKitModellingMatrixTranslationProperty : NestedProperty
    {
        private HPS.TransformMaskKit kit;
        private bool set;
        private bool _state;

        public TransformMaskKitModellingMatrixTranslationProperty(
            HPS.TransformMaskKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowModellingMatrixTranslation(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetModellingMatrixTranslation(_state);
            else
                kit.UnsetModellingMatrixTranslation();
        }
    }

    [ExpandableObject]
    public class TransformMaskKitModellingMatrixOffsetProperty : NestedProperty
    {
        private HPS.TransformMaskKit kit;
        private bool set;
        private bool _state;

        public TransformMaskKitModellingMatrixOffsetProperty(
            HPS.TransformMaskKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowModellingMatrixOffset(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetModellingMatrixOffset(_state);
            else
                kit.UnsetModellingMatrixOffset();
        }
    }

    [DisplayName("TransformMask")]
    public class TransformMaskKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.TransformMaskKit kit;

        public TransformMaskKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowTransformMask(out kit);

            CameraRotation = new TransformMaskKitCameraRotationProperty(kit);
            CameraScale = new TransformMaskKitCameraScaleProperty(kit);
            CameraTranslation = new TransformMaskKitCameraTranslationProperty(kit);
            CameraPerspectiveScale = new TransformMaskKitCameraPerspectiveScaleProperty(kit);
            CameraProjection = new TransformMaskKitCameraProjectionProperty(kit);
            CameraOffset = new TransformMaskKitCameraOffsetProperty(kit);
            CameraNearLimit = new TransformMaskKitCameraNearLimitProperty(kit);
            ModellingMatrixRotation = new TransformMaskKitModellingMatrixRotationProperty(kit);
            ModellingMatrixScale = new TransformMaskKitModellingMatrixScaleProperty(kit);
            ModellingMatrixTranslation = new TransformMaskKitModellingMatrixTranslationProperty(kit);
            ModellingMatrixOffset = new TransformMaskKitModellingMatrixOffsetProperty(kit);
        }

        [PropertyOrder(0)]
        public TransformMaskKitCameraRotationProperty CameraRotation { get; set; }

        [PropertyOrder(1)]
        public TransformMaskKitCameraScaleProperty CameraScale { get; set; }

        [PropertyOrder(2)]
        public TransformMaskKitCameraTranslationProperty CameraTranslation { get; set; }

        [PropertyOrder(3)]
        public TransformMaskKitCameraPerspectiveScaleProperty CameraPerspectiveScale { get; set; }

        [PropertyOrder(4)]
        public TransformMaskKitCameraProjectionProperty CameraProjection { get; set; }

        [PropertyOrder(5)]
        public TransformMaskKitCameraOffsetProperty CameraOffset { get; set; }

        [PropertyOrder(6)]
        public TransformMaskKitCameraNearLimitProperty CameraNearLimit { get; set; }

        [PropertyOrder(7)]
        public TransformMaskKitModellingMatrixRotationProperty ModellingMatrixRotation { get; set; }

        [PropertyOrder(8)]
        public TransformMaskKitModellingMatrixScaleProperty ModellingMatrixScale { get; set; }

        [PropertyOrder(9)]
        public TransformMaskKitModellingMatrixTranslationProperty ModellingMatrixTranslation { get; set; }

        [PropertyOrder(10)]
        public TransformMaskKitModellingMatrixOffsetProperty ModellingMatrixOffset { get; set; }

        public void Apply()
        {
            key.UnsetTransformMask();
            key.SetTransformMask(kit);
        }
    }
    #endregion

    #region ColorInterpolationKit
    [ExpandableObject]
    public class ColorInterpolationKitFaceColorProperty : NestedProperty
    {
        private HPS.ColorInterpolationKit kit;
        private bool set;
        private bool _state;

        public ColorInterpolationKitFaceColorProperty(
            HPS.ColorInterpolationKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowFaceColor(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetFaceColor(_state);
            else
                kit.UnsetFaceColor();
        }
    }

    [ExpandableObject]
    public class ColorInterpolationKitEdgeColorProperty : NestedProperty
    {
        private HPS.ColorInterpolationKit kit;
        private bool set;
        private bool _state;

        public ColorInterpolationKitEdgeColorProperty(
            HPS.ColorInterpolationKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowEdgeColor(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetEdgeColor(_state);
            else
                kit.UnsetEdgeColor();
        }
    }

    [ExpandableObject]
    public class ColorInterpolationKitVertexColorProperty : NestedProperty
    {
        private HPS.ColorInterpolationKit kit;
        private bool set;
        private bool _state;

        public ColorInterpolationKitVertexColorProperty(
            HPS.ColorInterpolationKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowVertexColor(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVertexColor(_state);
            else
                kit.UnsetVertexColor();
        }
    }

    [ExpandableObject]
    public class ColorInterpolationKitFaceIndexProperty : NestedProperty
    {
        private HPS.ColorInterpolationKit kit;
        private bool set;
        private bool _state;

        public ColorInterpolationKitFaceIndexProperty(
            HPS.ColorInterpolationKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowFaceIndex(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetFaceIndex(_state);
            else
                kit.UnsetFaceIndex();
        }
    }

    [ExpandableObject]
    public class ColorInterpolationKitEdgeIndexProperty : NestedProperty
    {
        private HPS.ColorInterpolationKit kit;
        private bool set;
        private bool _state;

        public ColorInterpolationKitEdgeIndexProperty(
            HPS.ColorInterpolationKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowEdgeIndex(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetEdgeIndex(_state);
            else
                kit.UnsetEdgeIndex();
        }
    }

    [ExpandableObject]
    public class ColorInterpolationKitVertexIndexProperty : NestedProperty
    {
        private HPS.ColorInterpolationKit kit;
        private bool set;
        private bool _state;

        public ColorInterpolationKitVertexIndexProperty(
            HPS.ColorInterpolationKit kit)
            : base(null)
        {
            this.kit = kit;
            set = this.kit.ShowVertexIndex(out _state);
            if (!set)
            {
                _state = true;
            }

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Set", typeof(bool), null, expandable: false, triggersRefresh: true));
            Properties.Add(new PropertySpec("State", typeof(bool), null, expandable: false, triggersRefresh: false));
            PropertyUtilities.SetBrowsable(Properties, set);
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set": e.Value = set; break;
                case "State": e.Value = _state; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Set":
                {
                    set = (bool)e.Value;
                    PropertyUtilities.SetBrowsable(Properties, set);
                }
                break;
                case "State":
                {
                    _state = (bool)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            if (set)
                kit.SetVertexIndex(_state);
            else
                kit.UnsetVertexIndex();
        }
    }

    [DisplayName("ColorInterpolation")]
    public class ColorInterpolationKitProperty : IRootProperty
    {
        private HPS.SegmentKey key;
        private HPS.ColorInterpolationKit kit;

        public ColorInterpolationKitProperty(
            HPS.SegmentKey key)
        {
            this.key = key;
            this.key.ShowColorInterpolation(out kit);

            FaceColor = new ColorInterpolationKitFaceColorProperty(kit);
            EdgeColor = new ColorInterpolationKitEdgeColorProperty(kit);
            VertexColor = new ColorInterpolationKitVertexColorProperty(kit);
            FaceIndex = new ColorInterpolationKitFaceIndexProperty(kit);
            EdgeIndex = new ColorInterpolationKitEdgeIndexProperty(kit);
            VertexIndex = new ColorInterpolationKitVertexIndexProperty(kit);
        }

        [PropertyOrder(0)]
        public ColorInterpolationKitFaceColorProperty FaceColor { get; set; }

        [PropertyOrder(1)]
        public ColorInterpolationKitEdgeColorProperty EdgeColor { get; set; }

        [PropertyOrder(2)]
        public ColorInterpolationKitVertexColorProperty VertexColor { get; set; }

        [PropertyOrder(3)]
        public ColorInterpolationKitFaceIndexProperty FaceIndex { get; set; }

        [PropertyOrder(4)]
        public ColorInterpolationKitEdgeIndexProperty EdgeIndex { get; set; }

        [PropertyOrder(5)]
        public ColorInterpolationKitVertexIndexProperty VertexIndex { get; set; }

        public void Apply()
        {
            key.UnsetColorInterpolation();
            key.SetColorInterpolation(kit);
        }
    }
    #endregion

    #region UpdateOptionsKit
    [ExpandableObject]
    public class UpdateOptionsKitUpdateTypeProperty : NestedProperty
    {
        private HPS.UpdateOptionsKit kit;
        private HPS.Window.UpdateType _type;

        public UpdateOptionsKitUpdateTypeProperty(
            HPS.UpdateOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowUpdateType(out _type);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Type", typeof(HPS.Window.UpdateType), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type": e.Value = _type; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type":
                {
                    _type = (HPS.Window.UpdateType)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetUpdateType(_type);
        }
    }

    [ExpandableObject]
    public class UpdateOptionsKitTimeLimitProperty : NestedProperty
    {
        private HPS.UpdateOptionsKit kit;
        private double _time_limit;

        public UpdateOptionsKitTimeLimitProperty(
            HPS.UpdateOptionsKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowTimeLimit(out _time_limit);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Time Limit", typeof(double), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Time Limit": e.Value = _time_limit; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Time Limit":
                {
                    _time_limit = (double)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetTimeLimit(_time_limit);
        }
    }

    [DisplayName("UpdateOptions")]
    public class UpdateOptionsKitProperty : IRootProperty
    {
        private HPS.WindowKey key;
        private HPS.UpdateOptionsKit kit;

        public UpdateOptionsKitProperty(
            HPS.WindowKey key)
        {
            this.key = key;
            this.key.ShowUpdateOptions(out kit);

            UpdateType = new UpdateOptionsKitUpdateTypeProperty(kit);
            TimeLimit = new UpdateOptionsKitTimeLimitProperty(kit);
        }

        [PropertyOrder(0)]
        public UpdateOptionsKitUpdateTypeProperty UpdateType { get; set; }

        [PropertyOrder(1)]
        public UpdateOptionsKitTimeLimitProperty TimeLimit { get; set; }

        public void Apply()
        {
            key.SetUpdateOptions(kit);
        }
    }
    #endregion

    #region GridKit
    [ExpandableObject]
    public class GridKitTypeProperty : NestedProperty
    {
        private HPS.GridKit kit;
        private HPS.Grid.Type _type;

        public GridKitTypeProperty(
            HPS.GridKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowType(out _type);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Type", typeof(HPS.Grid.Type), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type": e.Value = _type; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Type":
                {
                    _type = (HPS.Grid.Type)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetType(_type);
        }
    }

    [ExpandableObject]
    public class GridKitOriginProperty : NestedProperty
    {
        private HPS.GridKit kit;
        private HPS.Point _origin;
        private PointProperty _originProperty;

        public GridKitOriginProperty(
            HPS.GridKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowOrigin(out _origin);

            GetValue += Get;
            SetValue += Set;
            _originProperty = new PointProperty(_origin, Update_origin);
            Properties.Add(new PropertySpec("Origin", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Origin": e.Value = _originProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Origin":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetOrigin(_origin);
        }

        private void Update_origin(
            HPS.Point _origin)
        {
            this._origin = _origin;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class GridKitFirstPointProperty : NestedProperty
    {
        private HPS.GridKit kit;
        private HPS.Point _first_point;
        private PointProperty _first_pointProperty;

        public GridKitFirstPointProperty(
            HPS.GridKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowFirstPoint(out _first_point);

            GetValue += Get;
            SetValue += Set;
            _first_pointProperty = new PointProperty(_first_point, Update_first_point);
            Properties.Add(new PropertySpec("First Point", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "First Point": e.Value = _first_pointProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "First Point":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetFirstPoint(_first_point);
        }

        private void Update_first_point(
            HPS.Point _first_point)
        {
            this._first_point = _first_point;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class GridKitSecondPointProperty : NestedProperty
    {
        private HPS.GridKit kit;
        private HPS.Point _second_point;
        private PointProperty _second_pointProperty;

        public GridKitSecondPointProperty(
            HPS.GridKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowSecondPoint(out _second_point);

            GetValue += Get;
            SetValue += Set;
            _second_pointProperty = new PointProperty(_second_point, Update_second_point);
            Properties.Add(new PropertySpec("Second Point", typeof(PointProperty), null, expandable: true, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Second Point": e.Value = _second_pointProperty; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Second Point":
                {
                    // nothing to do
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetSecondPoint(_second_point);
        }

        private void Update_second_point(
            HPS.Point _second_point)
        {
            this._second_point = _second_point;
            UpdateKit();
        }
    }

    [ExpandableObject]
    public class GridKitFirstCountProperty : NestedProperty
    {
        private HPS.GridKit kit;
        private int _first_count;

        public GridKitFirstCountProperty(
            HPS.GridKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowFirstCount(out _first_count);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("First Count", typeof(int), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "First Count": e.Value = _first_count; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "First Count":
                {
                    _first_count = (int)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetFirstCount(_first_count);
        }
    }

    [ExpandableObject]
    public class GridKitSecondCountProperty : NestedProperty
    {
        private HPS.GridKit kit;
        private int _second_count;

        public GridKitSecondCountProperty(
            HPS.GridKit kit)
            : base(null)
        {
            this.kit = kit;
            this.kit.ShowSecondCount(out _second_count);

            GetValue += Get;
            SetValue += Set;
            Properties.Add(new PropertySpec("Second Count", typeof(int), null, expandable: false, triggersRefresh: false));
        }

        private void Get(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Second Count": e.Value = _second_count; break;
            }
        }

        private void Set(
            object sender,
            PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Second Count":
                {
                    _second_count = (int)e.Value;
                }
                break;
            }
            UpdateKit();
        }

        private void UpdateKit()
        {
            kit.SetSecondCount(_second_count);
        }
    }

    [DisplayName("Grid")]
    public class GridKitProperty : IRootProperty
    {
        private HPS.GridKey key;
        private HPS.GridKit kit;

        public GridKitProperty(
            HPS.GridKey key)
        {
            this.key = key;
            this.key.Show(out kit);

            Type = new GridKitTypeProperty(kit);
            Origin = new GridKitOriginProperty(kit);
            FirstPoint = new GridKitFirstPointProperty(kit);
            SecondPoint = new GridKitSecondPointProperty(kit);
            FirstCount = new GridKitFirstCountProperty(kit);
            SecondCount = new GridKitSecondCountProperty(kit);
            Priority = new PriorityProperty<HPS.GridKit>(kit.ShowPriority, kit.SetPriority, kit.UnsetPriority);
            UserData = new UserDataProperty<HPS.GridKit>(kit.ShowUserData, kit.SetUserData, kit.UnsetAllUserData);
        }

        [PropertyOrder(0)]
        public GridKitTypeProperty Type { get; set; }

        [PropertyOrder(1)]
        public GridKitOriginProperty Origin { get; set; }

        [PropertyOrder(2)]
        public GridKitFirstPointProperty FirstPoint { get; set; }

        [PropertyOrder(3)]
        public GridKitSecondPointProperty SecondPoint { get; set; }

        [PropertyOrder(4)]
        public GridKitFirstCountProperty FirstCount { get; set; }

        [PropertyOrder(5)]
        public GridKitSecondCountProperty SecondCount { get; set; }

        [PropertyOrder(6)]
        public PriorityProperty<HPS.GridKit> Priority { get; set; }

        [PropertyOrder(7)]
        public UserDataProperty<HPS.GridKit> UserData { get; set; }

        public void Apply()
        {
            key.Set(kit);
        }
    }
    #endregion

}
