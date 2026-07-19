using OnlyDarker.CommonUsing;
using OnlyDarker.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public interface IInteractive
    {
        string IngameName { get;}
        static string BaseMessage => $"[{GameBody.GetGameInstance().ControlsManager.BindManager.Interact.Key}] to ";
        string InteractionMessage { get; }
        bool IsInteractive { get; set; }
        Rectangle MovementCollider { get; }
        virtual void ShowInteractionMessage()
        {
            InteractionMessageBar.GetInstance().PushMessage(BaseMessage + InteractionMessage + IngameName);
        }
        void Interact();
    }
}
