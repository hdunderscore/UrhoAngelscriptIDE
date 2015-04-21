using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace XMLEditor.ViewModels
{
    public class TreeEditorsViewModel : BaseViewModel
    {
        private int activeTabIndex;
        private ObservableCollection<TreeEditorViewModel> treeEditors = new ObservableCollection<TreeEditorViewModel>();

        private TreeEditorViewModel activeEditor;

        public TreeEditorViewModel ActiveEditor
        {
            get { return activeEditor; }
            set
            {
                activeEditor = value;
                OnPropertyChanged("ActiveEditor");
            }
        }


        public int ActiveEditorIndex
        {
            get { return activeEditorIndex; }
            set
            {
                activeEditorIndex = value;
                OnPropertyChanged("ActiveEditorIndex");
                if (activeEditorIndex > -1 && activeEditorIndex < TreeEditors.Count)
                {
                    ActiveEditor = TreeEditors[ActiveEditorIndex];

                }
            }
        }


        public ObservableCollection<TreeEditorViewModel> TreeEditors
        {
            get { return treeEditors; }
            set
            {
                treeEditors = value;
                OnPropertyChanged("TreeEditors");
            }
        }

        public void Add(TreeEditorViewModel treeEditor)
        {
            this.TreeEditors.Add(treeEditor);
            ActiveEditorIndex = this.TreeEditors.Count - 1;

        }
        public void Remove(TreeEditorViewModel treeEditor)
        {
            treeEditor.UnloadEditor();
            this.TreeEditors.Remove(treeEditor);
            ActiveEditorIndex = this.TreeEditors.Count - 1;
        }

        public int activeEditorIndex { get; set; }
    }
}
