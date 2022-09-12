using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Network;
using Eto;
using Pablo.Formats.Character.Tools;

namespace Pablo.Formats.Character.Actions.Block
{
	public class Delete : PabloCommand
	{
		Selection tool;
		public new CharacterHandler Handler { get { return base.Handler as CharacterHandler; } }

		public override bool AllowKeyboardFallback => true;

		public const string ActionID = "character_BlockDelete";
		
		public Delete (Selection tool) : base(tool.Handler)
		{
			this.tool = tool;
			ID = ActionID;
			MenuText = "&Delete";
			ToolTip = "Deletes the selected region";
			Name = "Paste";
			Shortcut = Keys.Delete; //new [] { Keys.Delete, Keys.E };
		}

		public override bool Enabled
		{
			get { return base.Enabled && tool.DrawMode == DrawMode.Selecting; }
			set { base.Enabled = value; }
		}

		public override int CommandID {
			get { return (int)NetCommands.BlockDelete; }
		}
		
		public override UserLevel Level {
			get { return UserLevel.Editor; }
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			var handler = this.Handler;
			
			var rect = tool.SelectedRegion.Value;
			Do (rect, handler.CursorPosition, handler.DrawElement);
			
			tool.DrawMode = DrawMode.Normal;
			tool.SelectedRegion = null;
			handler.CursorPosition = rect.TopLeft;
		}
		
		void Do (Rectangle rect, Point? pos, CanvasElement element)
		{
			rect.Normalize ();
			
			this.Handler.Undo.Save (pos, pos, rect);
			
			this.Handler.CurrentPage.Canvas.Fill (rect, element);
			this.Handler.InvalidateCharacterRegion(rect, true);
		}
		
		public override bool Send (Pablo.Network.SendCommandArgs args)
		{
			base.Send (args);
			var rect = tool.SelectedRegion.Value;
			args.Message.Write (rect);
			args.Message.Write (Handler.DrawElement);
			tool.DrawMode = DrawMode.Normal;
			tool.SelectedRegion = null;
			tool.Handler.CursorPosition = rect.TopLeft;
			return true;
		}
		
		public override void Receive (Pablo.Network.ReceiveCommandArgs args)
		{
			base.Receive (args);
			var rect = args.Message.ReadRectangle ();
			var element = args.Message.ReadCanvasElement ();
			args.Invoke (delegate {
				Do (rect, null, element);
			});
		}
	}
}

