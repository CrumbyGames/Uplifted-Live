using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// A utility singleton that builds upon Godot's node system.
/// </summary>
public partial class @Global : Node
{
	/// <summary>
    /// Creates a new instance of a specified node type and adds it as a child to a node. 
    /// </summary>
    /// <returns>
    /// A reference to the instance.
    /// </returns>
    /// <typeparam name="NodeType">The type of node to be instanced.</typeparam>
    /// <param name="parent">The intended parent of the instance.</param>
    /// <param name="properties">Initial properties of the instance.</param>
    /// <param name="index">The index of the instance among the other children. -1 ignored.</param>
	
	public NodeType AddInstanceAsChild<NodeType>(Node parent, Dictionary<string, Godot.Variant> properties = null, int index = -1) where NodeType : Node, new()
	{
		// Instance the node
        NodeType child = new NodeType();
		
		// Assign initial properties
		foreach(string property in properties.Keys) {
            child.Set(property, properties[property]);
        }

		// Add instance to tree
        parent.AddChild(child);

		// Move instance in tree if needed
        if (index != -1)
        {
            parent.MoveChild(child, index);
        }

		// Return reference to instance.
        return child;
    }
}
