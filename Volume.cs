using Aveva.Core.Database;
using Aveva.Core.Geometry;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Aveva.ApplicationFramework;

namespace VolumeWizardAddin
{
    public class DbElementChangesEventArgs : EventArgs
    {
        public DbElementChangesEventArgs() { }
        public DbElementChangesEventArgs(DbElement element) => Element = element;
        DbElement Element { get; set; }
    }
    public abstract class Volume : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DbElementChangesEventArgs> DbElementDeleted;
        public event EventHandler<DbElementChangesEventArgs> OwnerDeleted;

        protected DbElement _element;
        protected DbElement _owner;
        protected Position _position;
        string _name;

        protected void PropChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public Volume(DbElement element) => Element = element;

        public void ChangeEventHandler(DbUserChanges changes)
        {
            if (changes.GetDeletions().Contains(_owner))
            {
                OwnerDeleted?.Invoke(this, new DbElementChangesEventArgs(_element.Owner));
                PropChanged(null);
                return;
            }
            if (changes.GetDeletions().Contains(_element))
            {
                DbElementDeleted?.Invoke(this, new DbElementChangesEventArgs(_element));
                PropChanged(null);
            }
        }

        public virtual DbElement Element
        {
            get { return _element; }
            set
            {
                _element = value;
                _owner = Element.Owner;          
                _name = _element.Name();
                _position = _element.GetPosition(DbAttributeInstance.POS);

                DbEvents.AddHandleUserChanges(ChangeEventHandler);

                PropChanged(null);
            }
        }

        public double PosX
        {
            get { return _position.X; }
            set
            {
                _position.X = value;
                _element.SetAttribute(DbAttributeInstance.POS, _position);
                PropChanged("PosX");
            }
        }
        public double PosY
        {
            get { return _position.Y; }
            set
            {
                _position.Y = value;
                _element.SetAttribute(DbAttributeInstance.POS, _position);
                PropChanged("PosY");
            }
        }
        public double PosZ
        {
            get { return _position.Z; }
            set
            {
                _position.Z = value;
                _element.SetAttribute(DbAttributeInstance.POS, _position);
                PropChanged("PosZ");
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                _element.SetAttribute(DbAttributeInstance.NAME, _name);
                PropChanged("Name");
            }
        }

        public void Delete()
        {
            _element.Delete();
            DbElementDeleted?.Invoke(this, new DbElementChangesEventArgs(_element));
        }
    }

    public class MainVolume : Volume, INotifyPropertyChanged
    {
        protected DbDouble _lenX, _lenY, _lenZ;

        public MainVolume(DbElement element) : base(element)
        {
            _lenX = _element.GetDbDouble(DbAttributeInstance.XLEN);
            _lenY = _element.GetDbDouble(DbAttributeInstance.YLEN);
            _lenZ = _element.GetDbDouble(DbAttributeInstance.ZLEN);
            base.PropChanged("LenX");
            base.PropChanged("LenY");
            base.PropChanged("LenZ");
        }

        public string LenXunits => _lenX.Units.ShortName;
        public string LenYunits => _lenY.Units.ShortName;
        public string LenZunits => _lenZ.Units.ShortName;

        public double LenX
        {
            get { return _lenX.Value; }
            set
            {
                _lenX = DbDouble.Create(value);
                _element.SetAttribute(DbAttributeInstance.XLEN, _lenX);
                base.PropChanged("LenX");
            }
        }
        public double LenY
        {
            get { return _lenY.Value; }
            set
            {
                _lenY = DbDouble.Create(value);
                _element.SetAttribute(DbAttributeInstance.YLEN, _lenY);
                base.PropChanged("LenY");
            }
        }
        public double LenZ
        {
            get { return _lenZ.Value; }
            set
            {
                _lenZ = DbDouble.Create(value);
                _element.SetAttribute(DbAttributeInstance.ZLEN, _lenZ);
                base.PropChanged("LenZ");
            }
        }
        public string LenXasString
        {
            get { return _lenX.ToString(); }
            set
            {
                try
                {
                    _lenX = DbDouble.Create(value);
                    _element.SetAttribute(DbAttributeInstance.XLEN, _lenX);
                    base.PropChanged("LenXasString");
                }
                catch { }
            }
        }
        public string LenYasString
        {
            get { return _lenY.ToString(); }
            set
            {
                try
                {
                    _lenY = DbDouble.Create(value);
                    _element.SetAttribute(DbAttributeInstance.YLEN, _lenY);
                    base.PropChanged("LenYasString");
                }
                catch { }
            }
        }
        public string LenZasString
        {
            get { return _lenZ.ToString(); }
            set
            {
                try
                {
                    _lenZ = DbDouble.Create(value);
                    _element.SetAttribute(DbAttributeInstance.ZLEN, _lenZ);
                    base.PropChanged("LenYasString");
                }
                catch { }
            }
        }
    }

    public class View : Volume
    {
        Orientation _orientation;
        Direction _direction;
        DbElement _mainElement;

        public View(DbElement element, DbElement mainElement) : base(element) => MainElement = mainElement;

        public override DbElement Element
        {
            get { return _element; }
            set
            {
                base.Element = value;
                MainElement = _mainElement;

                base.PropChanged(null);
            }
        }

        public DbElement MainElement
        {
            get { return _mainElement; }
            set
            {
                _mainElement = value;
                _orientation = _element.GetOrientation(DbAttributeInstance.ORI, new DbQualifier() { wrtQualifier = _mainElement });
                _direction = _orientation.ZDir();
                base.PropChanged("MainElement");
            }
        }

        public string ViewDirection
        {
            get { return _direction.ToString(); }
            set
            {
                if (Direction.Parse(value, out Direction zDir, out _) && zDir != _direction)
                {
                    _direction = zDir;

                    Direction yDir = Direction.Create();
                    yDir.North = zDir.North;
                    yDir.East = zDir.East;
                    yDir.Up = zDir.Up + 1;

                    _orientation = Orientation.Create(Axis.Z, zDir, Axis.Y, yDir);
                    _element.EvaluateValidOrientation(DbExpression.Parse($"{_orientation} WRT {_mainElement.Name()}"), ref _orientation);
                    _element.SetAttribute(DbAttributeInstance.ORI, _orientation);

                    Direction oDir = _element.GetOrientation(DbAttributeInstance.ORI, new DbQualifier() { wrtQualifier = _mainElement }).ZDir();
                    //Position position = Position.Create();

                    Position position = _mainElement.GetPosition(DbAttributeInstance.POS);

                    double h = _element.GetDouble(DbAttributeInstance.HEIG);
                    double x = (h + _mainElement.GetDouble(DbAttributeInstance.XLEN)) / 2;
                    double y = (h + _mainElement.GetDouble(DbAttributeInstance.YLEN)) / 2;
                    double z = (h + _mainElement.GetDouble(DbAttributeInstance.ZLEN)) / 2;

                    //position.X += x * oDir.East;
                    //position.Y += y * oDir.North;
                    //position.Z += z * oDir.Up;

                    Position.Parse($"E {Math.Round(x * oDir.East, 3)} " +
                    $"N {Math.Round(y * oDir.North, 3)} U {Math.Round(z * oDir.Up, 3)} WRT {_mainElement.Name()}", out position, out _);

                    //_element.EvaluateValidPosition(DbExpression.Parse($"E {Math.Round (x * oDir.East, 3)} " +
                    //$"N {Math.Round(y * oDir.North, 3)} U {Math.Round(z * oDir.Up,3)} WRT {_mainElement.Name()}"), ref position);

                    _element.SetAttribute(DbAttributeInstance.POS, position);

                    base.PropChanged("ViewDirection");

                }
            }
        }

    }

    public class Section : MainVolume
    {
        DbElement _dirElement;
        View _view;

        public Section(DbElement element) : base(element) { }

        public override DbElement Element
        {
            get { return _element; }
            set
            {
                base.Element = value;
                _dirElement = _element.Members().Where(x => x.ElementType == DbElementTypeInstance.NPYRAMID).FirstOrDefault();
                if (_dirElement != null) _view = new View(_dirElement, _element);
            }
        }

        public string ViewDirection
        {
            get { return _view?.ViewDirection.ToString(); }
            set
            {
                if (_view != null) _view.ViewDirection = value;
                base.PropChanged("ViewDirection");
            }
        }
    }
}