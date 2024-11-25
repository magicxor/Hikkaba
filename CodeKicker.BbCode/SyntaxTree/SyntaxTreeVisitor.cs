﻿namespace CodeKicker.BbCode.SyntaxTree;

public class SyntaxTreeVisitor
{
    public SyntaxTreeNode Visit(SyntaxTreeNode node)
    {
        return node == null
            ? null
            : node.AcceptVisitor(this);
    }

    protected internal virtual SyntaxTreeNode Visit(SequenceNode node)
    {
        if (node == null)
        {
            return null;
        }

        var modifiedSubNodes = GetModifiedSubNodes(node);

        return modifiedSubNodes == null
            ? node // unmodified
            : node.SetSubNodes(modifiedSubNodes); // subnodes were modified
    }

    protected internal virtual SyntaxTreeNode Visit(TagNode node)
    {
        if (node == null)
        {
            return null;
        }

        var modifiedSubNodes = GetModifiedSubNodes(node);

        return modifiedSubNodes == null
            ? node // unmodified
            : node.SetSubNodes(modifiedSubNodes); // subnodes were modified
    }

    protected internal virtual SyntaxTreeNode Visit(TextNode node)
    {
        return node;
    }

    private SyntaxTreeNodeCollection GetModifiedSubNodes(SyntaxTreeNode node)
    {
        SyntaxTreeNodeCollection modifiedSubNodes = null; //lazy

        for (int i = 0; i < node.SubNodes.Count; i++)
        {
            var subNode = node.SubNodes[i];

            var replacement = Visit(subNode);
            if (replacement != subNode)
            {
                if (modifiedSubNodes == null) //lazy init
                {
                    modifiedSubNodes = new SyntaxTreeNodeCollection();
                    for (int j = 0; j < i; j++) //copy unmodified nodes
                        modifiedSubNodes.Add(node.SubNodes[j]);
                }

                if (replacement != null) //insert replacement
                {
                    modifiedSubNodes.Add(replacement);
                }
            }
            else
            {
                if (modifiedSubNodes != null) //only insert unmodified subnode if the lazy collection has been initialized
                {
                    modifiedSubNodes.Add(subNode);
                }
            }
        }
        return modifiedSubNodes;
    }
}
