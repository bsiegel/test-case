using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
namespace TestCase
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        private TextDocumentKeyPressEvents _textDocKeyEvents;
        private Commands2 _commands;
        private bool isOn;
        private bool nextShouldBeCaps;
        private const string fullCmdName = "TestCase.Connect.Toggle";

        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;            
            _textDocKeyEvents = ((Events2) _applicationObject.Events).get_TextDocumentKeyPressEvents();
            _commands = (Commands2) _applicationObject.Commands;

            try {
                _commands.AddNamedCommand2(_addInInstance, "Toggle", "Toggle TestCase", "Toggle test naming replacements on or off", false);
            } catch (Exception) {
            }

            _textDocKeyEvents.BeforeKeyPress += new _dispTextDocumentKeyPressEvents_BeforeKeyPressEventHandler(BeforeKeyPress);
        }


        // toggling isOn to true should make nextShouldBeCaps true
        void ToggleIsOn() {
            isOn = !isOn;
            nextShouldBeCaps = isOn;
        }

        void BeforeKeyPress(string Keypress, TextSelection Selection, bool InStatementCompletion, ref bool CancelKeypress) {
            if (isOn) {
                bool isAlpha = (Keypress[0] >= 'A' && Keypress[0] <= 'Z') || (Keypress[0] >= 'a' && Keypress[0] <= 'z');
                bool swap = Selection.IsActiveEndGreater;
                if (isAlpha) {
                    if (!nextShouldBeCaps) {
                        if (swap)
                            Selection.SwapAnchor();
                        Selection.CharLeft(true);
                        if (Selection.Text[0] == '_')
                            nextShouldBeCaps = true;
                        Selection.CharRight(true);
                        if (swap)
                            Selection.SwapAnchor();
                    }
                    if (nextShouldBeCaps) {
                        CancelKeypress = true;
                        if (!Selection.IsEmpty) {
                            Selection.Delete();
                        }
                        Selection.Insert(Keypress.ToUpper());
                    }
                } else if (Keypress == " ") {
                    CancelKeypress = true;
                    if (!Selection.IsEmpty) {
                        Selection.Delete();
                    }
                    Selection.Insert("_");
                } else if (Keypress == "(") {
                    CancelKeypress = false;
                    ToggleIsOn();
                }
                nextShouldBeCaps = false;
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            if (_textDocKeyEvents != null) {
                _textDocKeyEvents.BeforeKeyPress -= new _dispTextDocumentKeyPressEvents_BeforeKeyPressEventHandler(BeforeKeyPress);
            }
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }
        
        private DTE2 _applicationObject;
        private AddIn _addInInstance;

        #region IDTCommandTarget Members

        public void Exec(string CmdName, vsCommandExecOption ExecuteOption, ref object VariantIn, ref object VariantOut, ref bool Handled) {
            if (ExecuteOption != vsCommandExecOption.vsCommandExecOptionDoDefault || _applicationObject.ActiveDocument == null || CmdName != fullCmdName) {
                return;
            }
            Handled = true;
            ToggleIsOn();
        }

        public void QueryStatus(string CmdName, vsCommandStatusTextWanted NeededText, ref vsCommandStatus StatusOption, ref object CommandText) {
            if (NeededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone) {
                if (_applicationObject.ActiveDocument == null)
                    StatusOption = vsCommandStatus.vsCommandStatusUnsupported;
                else
                    StatusOption = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
            }
        }
        #endregion
    }
}