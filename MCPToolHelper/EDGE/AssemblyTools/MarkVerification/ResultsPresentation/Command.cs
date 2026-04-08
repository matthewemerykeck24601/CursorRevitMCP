// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Diagnostics;
using System.Windows.Input;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class Command : ICommand
{
  private Command.ICommandOnExecute _execute;
  private Command.ICommandOnCanExecute _canExecute;

  public Command(
    Command.ICommandOnExecute onExecuteMethod,
    Command.ICommandOnCanExecute onCanExecuteMethod)
  {
    this._execute = onExecuteMethod;
    this._canExecute = onCanExecuteMethod;
  }

  public event EventHandler CanExecuteChanged
  {
    add => CommandManager.RequerySuggested += value;
    remove => CommandManager.RequerySuggested -= value;
  }

  [DebuggerHidden]
  public bool CanExecute(object parameter) => this._canExecute(parameter);

  [DebuggerHidden]
  public void Execute(object parameter) => this._execute(parameter);

  public delegate void ICommandOnExecute(object parameter);

  public delegate bool ICommandOnCanExecute(object parameter);
}
