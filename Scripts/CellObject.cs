using Godot;
using System;

[GlobalClass]
public partial class CellObject : Resource
{
    [Export] internal PackedScene Object;
    [ExportGroup("Sockets")] 
    [Export] internal String PosX = "Air";
    [Export] internal String PosY = "Air";
    [Export] internal String PosZ = "Air";
    [Export] internal String NegX = "Air";
    [Export] internal String NegY = "Air";
    [Export] internal String NegZ = "Air";
    
    internal bool Rotate90;
    internal bool Rotate180;
}
