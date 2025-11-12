using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility para criar rapidamente paredes invisíveis
/// Menu: GameObject -> 2D Object -> Invisible Wall
/// </summary>
public class InvisibleWallCreator : Editor
{
    [MenuItem("GameObject/2D Object/Invisible Wall", false, 10)]
    static void CreateInvisibleWall(MenuCommand menuCommand)
    {
        // Cria o GameObject
        GameObject wall = new GameObject("InvisibleWall");
        
        // Adiciona o BoxCollider2D
        BoxCollider2D boxCollider = wall.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = false;
        
        // Configura o tamanho padrão (1 unidade de largura, 20 de altura)
        boxCollider.size = new Vector2(1f, 20f);
        
        // Adiciona o script InvisibleWall
        wall.AddComponent<InvisibleWall>();
        
        // Configura a layer como "Default" ou "Ground" se existir
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1)
        {
            wall.layer = groundLayer;
        }
        
        // Registra o objeto para Undo
        Undo.RegisterCreatedObjectUndo(wall, "Create Invisible Wall");
        
        // Define o parent baseado no contexto
        GameObjectUtility.SetParentAndAlign(wall, menuCommand.context as GameObject);
        
        // Seleciona o novo objeto
        Selection.activeObject = wall;
        
        Debug.Log("InvisibleWall criada! Posicione-a nos limites do mapa.");
    }

    [MenuItem("GameObject/2D Object/Invisible Wall Setup (Left + Right)", false, 11)]
    static void CreateInvisibleWallPair(MenuCommand menuCommand)
    {
        // Cria um container
        GameObject container = new GameObject("InvisibleWalls");
        
        // Parede da Esquerda
        GameObject leftWall = new GameObject("InvisibleWall_Left");
        leftWall.transform.parent = container.transform;
        leftWall.transform.position = new Vector3(-10f, 10f, 0f); // Posição padrão à esquerda
        
        BoxCollider2D leftCollider = leftWall.AddComponent<BoxCollider2D>();
        leftCollider.size = new Vector2(1f, 20f);
        leftCollider.isTrigger = false;
        leftWall.AddComponent<InvisibleWall>();
        
        // Parede da Direita
        GameObject rightWall = new GameObject("InvisibleWall_Right");
        rightWall.transform.parent = container.transform;
        rightWall.transform.position = new Vector3(100f, 10f, 0f); // Posição padrão à direita
        
        BoxCollider2D rightCollider = rightWall.AddComponent<BoxCollider2D>();
        rightCollider.size = new Vector2(1f, 20f);
        rightCollider.isTrigger = false;
        rightWall.AddComponent<InvisibleWall>();
        
        // Registra para Undo
        Undo.RegisterCreatedObjectUndo(container, "Create Invisible Wall Pair");
        
        // Define o parent
        GameObjectUtility.SetParentAndAlign(container, menuCommand.context as GameObject);
        
        // Seleciona o container
        Selection.activeObject = container;
        
        Debug.Log("Par de InvisibleWalls criado! Ajuste as posições X para os limites do seu mapa.");
    }
}

