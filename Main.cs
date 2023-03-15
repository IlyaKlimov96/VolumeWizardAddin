using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aveva.Core.Database;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Aveva.Core.Geometry;
using System.Windows;

namespace VolumeWizardAddin
{
    public class ListOfDRWG : INotifyPropertyChanged
    {
        public ObservableCollection<DRWG> GetList { get; } = new ObservableCollection<DRWG>();

        private DbElement _element;

        public event PropertyChangedEventHandler PropertyChanged;

        public DbElement Element { get { return _element; } }

        public string Name { get { return _element.Name(); } }

        public void Update(DbElement element = null)
        {
            if (element == null) element = CurrentElement.Element;
            if (element.GetElementType() == DbElementTypeInstance.ZONE)
            {
                GetList.Clear();
                foreach (DbElement member in element.Members())
                {
                    if (member.GetElementType() == DbElementTypeInstance.EQUIPMENT && member.GetAsString(DbAttributeInstance.PURP) == "DRWG") GetList.Add(new DRWG(member));
                }
                _element = element;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }

        public DRWG Create()
        {
            DbElement element = _element.CreateLast(DbElementTypeInstance.EQUIPMENT);
            for (int i = 1; !element.GetBool(DbAttributeInstance.ISNAME); i++)
            {
                if (!DbElement.Parse($"{_element.Name()}/DRWG{i}", out _, out _)) element.SetAttribute(DbAttributeInstance.NAME, $"{_element.Name()}/DRWG{i}");
            }
            DRWG dRWG = new DRWG(element);
            this.GetList.Add(dRWG);
            return dRWG;
        }
    }


    public class DRWG : INotifyPropertyChanged
    {

        DbElement _element, _subeMain, _subeViews, _subeSections;

        MainVolume _main;

        public ObservableCollection<View> Views { get; private set; } = new ObservableCollection<View>();
        public ObservableCollection<Section> Sections { get; private set; } = new ObservableCollection<Section>();

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return _element.Name(); }
            set
            {
                if (!value.StartsWith("/")) value = "/" + value;
                try
                {
                    _element.SetAttribute(DbAttributeInstance.NAME, value);
                }
                catch { }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public DRWG(DbElement element)
        {
            Element = element;
        }

        public DbElement Element
        {
            get { return _element; }
            set
            {
                _element = value;
                _subeMain = _element.Members().Where(x => x.Name() == $"{_element.Name()}/MAIN").FirstOrDefault();
                DbElement subeMainBox = _subeMain?.Members().Where(x => x.GetElementType() == DbElementTypeInstance.BOX).FirstOrDefault();
                if (subeMainBox != null)
                {
                    _main = new MainVolume(subeMainBox);
                    _main.DbElementDeleted += main_Deleted;
                    _main.OwnerDeleted += main_OwnerDeleted;
                }

                Views.Clear();
                _subeViews = _element.Members().Where(x => x.Name() == $"{_element.Name()}/VIEWS").FirstOrDefault();
                if (_main != null && _subeViews != null)
                {
                    foreach (DbElement element in _subeViews?.Members().Where(x => x.GetElementType() == DbElementTypeInstance.CONE))
                    {
                        View view = new View(element, _main.Element);
                        view.DbElementDeleted += View_Deleted;
                        view.OwnerDeleted += View_OwnerDeleted;
                        Views.Add(view);
                    }
                }

                Sections.Clear();
                _subeSections = _element.Members().Where(x => x.Name() == $"{_element.Name()}/SECTIONS").FirstOrDefault();
                if (_subeSections != null)
                {
                    foreach (DbElement element in _subeSections?.Members().Where(x => x.GetElementType() == DbElementTypeInstance.BOX))
                    {
                        Section section = new Section(element);
                        section.DbElementDeleted += Section_Deleted;
                        section.OwnerDeleted += Section_OwnerDeleted;
                        Sections.Add(section);
                    }
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }

        #region Events
        private void Section_Deleted(object sender, DbElementChangesEventArgs e) => Sections.Remove((Section)sender);

        private void Section_OwnerDeleted(object sender, DbElementChangesEventArgs e)
        {
            _subeSections = null;
            Sections.Clear();
        }
        private void main_Deleted(object sender, DbElementChangesEventArgs e)
        {
            _main = null;
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("MainVol"));
        }
        private void main_OwnerDeleted(object sender, DbElementChangesEventArgs e)
        {
            _main = null;
            _subeMain = null;
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("MainVol"));
        }
        private void View_Deleted(object sender, DbElementChangesEventArgs e) => Views.Remove((View)sender);

        private void View_OwnerDeleted(object sender, DbElementChangesEventArgs e)
        {
            _subeViews = null;
            Views.Clear();
        }
        #endregion


        public MainVolume MainVol { get { return _main; } }

        public void CreateMain()
        {
            if (_main == null)
            {
                if (_subeMain == null || !_subeMain.IsValid)
                {
                    if (!_element.GetOrCreateMember($"{_element.Name()}/MAIN", DbElementTypeInstance.SUBEQUIPMENT, out _subeMain))
                    {
                        MessageBox.Show("Не удалось создать объём");
                        return;
                    }
                }
                DbElement box = _subeMain.Members().Where(x => x.GetElementType() == DbElementTypeInstance.BOX).FirstOrDefault();
                if (box == null) _main = new MainVolume(_subeMain.Create(1, DbElementTypeInstance.BOX));
                else _main = new MainVolume(box);
            }
        }

        public void CreateView()
        {
            if (_subeViews == null || _subeViews.IsValid)
            {
                if (!_element.GetOrCreateMember($"{_element.Name()}/VIEWS", DbElementTypeInstance.SUBEQUIPMENT, out _subeViews))
                {
                    MessageBox.Show("Не удалось создать вид");
                    return;
                }
            }
            DbElement cone = _subeViews.Create(1, DbElementTypeInstance.CONE);
            for (char i = 'A'; !cone.GetBool(DbAttributeInstance.ISNAME); i++)
            {
                string name = $"{_subeViews.Name()}/{i}";
                if (!DbElement.Parse(name, out _, out _)) cone.SetAttribute(DbAttributeInstance.NAME, name);
            }
            View view = new View(cone, _element);
            view.DbElementDeleted += View_Deleted;
            view.OwnerDeleted += View_OwnerDeleted;
            Views.Add(view);
        }
        public void DeleteView (View view)
        {
            try
            {
               view.Element.Delete();
            }
            catch { }
        }

        public void CreateSection()
        {
            if (_subeSections == null || _subeSections.IsValid)
            {
                if (!_element.GetOrCreateMember($"{_element.Name()}/SECTIONS", DbElementTypeInstance.SUBEQUIPMENT, out _subeSections))
                {
                    MessageBox.Show("Не удалось создать разрезы");
                    return;
                }
            }
            DbElement box = _subeSections.Create(1, DbElementTypeInstance.BOX);
            box.Create(1, DbElementTypeInstance.NPYRAMID);
            for (char i = 'A'; !box.GetBool(DbAttributeInstance.ISNAME); i++)
            {
                string name = $"{_subeSections.Name()}/{i}";
                if (!DbElement.Parse(name, out _, out _)) box.SetAttribute(DbAttributeInstance.NAME, name);
            }
            Section section = new Section(box);
            section.DbElementDeleted += Section_Deleted;
            section.OwnerDeleted += Section_OwnerDeleted;
            Sections.Add(section);
        }
        public void DeleteSection(Section section)
        {
            try
            {
                section.Element.Delete();
            }
            catch { }
        }
    }


}
