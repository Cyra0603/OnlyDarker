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
        string Name { get;}
        static string BaseMessage => $"[{GameBody.GetGameInstance().ControlsManager.BindManager.Interact.Key}] to ";
        string InteractionMessage { get; }
        Rectangle MovementCollider { get; }
        void ShowInteractionMessage()
        {
            InteractionMessageBar.GetInstance().PushMessage(BaseMessage + InteractionMessage + Name);
        }
        void Interact();
    }
}
