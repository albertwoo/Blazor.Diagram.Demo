using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Events;
using Blazor.Diagrams.Core.Models.Base;

namespace Blazor.Diagram.Demo.Client.Pages.Dashboard.Behaviors;

internal class SelectionBehavior2 : Behavior
{
    public SelectionBehavior2(Diagrams.Core.Diagram diagram) : base(diagram)
    {
        Diagram.PointerDown += OnPointerDown;
    }

    private void OnPointerDown(Model? model, PointerEventArgs e)
    {
        if (e.Button == 2) return;

        bool ctrlKey = e.CtrlKey;
        if (model != null)
        {
            if (model is not SelectableModel selectableModel)
            {
                return;
            }

            if (ctrlKey && selectableModel.Selected)
            {
                Diagram.UnselectModel(selectableModel);
                return;
            }

            SelectableModel selectableModel2 = selectableModel;
            if (!selectableModel2.Selected)
            {
                Diagram.SelectModel(selectableModel2, !ctrlKey || !Diagram.Options.AllowMultiSelection);
            }
        }
        else
        {
            Diagram.UnselectAll();
        }
    }

    public override void Dispose()
    {
        Diagram.PointerDown -= OnPointerDown;
    }
}
