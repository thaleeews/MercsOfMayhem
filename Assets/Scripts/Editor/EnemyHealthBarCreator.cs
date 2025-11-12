using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using MercsOfMayhem.Enemies;

[InitializeOnLoad]
public class EnemyHealthBarCreator : Editor
{
    [MenuItem("GameObject/Mercs of Mayhem/Add Health Bar to Enemy", false, 0)]
    static void AddHealthBarToEnemy(MenuCommand menuCommand)
    {
        GameObject enemyObj = Selection.activeGameObject;
        
        if (enemyObj == null)
        {
            EditorUtility.DisplayDialog("Erro", "Selecione um GameObject com o componente Enemy primeiro!", "OK");
            return;
        }
        
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy == null)
        {
            EditorUtility.DisplayDialog("Erro", "O GameObject selecionado não tem um componente Enemy!", "OK");
            return;
        }
        
        // Verifica se já tem uma HealthBar
        EnemyHealthBar existingHealthBar = enemyObj.GetComponentInChildren<EnemyHealthBar>();
        if (existingHealthBar != null)
        {
            EditorUtility.DisplayDialog("Aviso", "Este inimigo já tem uma HealthBar!", "OK");
            return;
        }
        
        // Cria o Canvas
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(enemyObj.transform);
        canvasObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 1f); // Escala pequena para ficar proporcional
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        
        // Cria o Background da barra
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        bgObj.transform.localPosition = Vector3.zero;
        bgObj.transform.localScale = Vector3.one;
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(100f, 10f);
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Cinza escuro
        
        // Cria a barra de Fill (verde)
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(bgObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localScale = Vector3.one;
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = new Vector2(2, 2); // Padding
        fillRect.offsetMax = new Vector2(-2, -2); // Padding
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        
        // Adiciona o componente EnemyHealthBar
        EnemyHealthBar healthBar = canvasObj.AddComponent<EnemyHealthBar>();
        
        // Configura as referências via SerializedObject para funcionar no Editor
        SerializedObject serializedHealthBar = new SerializedObject(healthBar);
        serializedHealthBar.FindProperty("enemy").objectReferenceValue = enemy;
        serializedHealthBar.FindProperty("fillImage").objectReferenceValue = fillImage;
        serializedHealthBar.FindProperty("healthBarCanvas").objectReferenceValue = canvasObj;
        serializedHealthBar.ApplyModifiedProperties();
        
        // Registra para o Undo
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Enemy Health Bar");
        
        EditorUtility.DisplayDialog("Sucesso!", "Barra de vida criada com sucesso!\n\nVocê pode ajustar a posição e configurações no Inspector.", "OK");
        
        // Seleciona o canvas criado
        Selection.activeGameObject = canvasObj;
    }
    
    // Valida se o menu item deve estar ativo
    [MenuItem("GameObject/Mercs of Mayhem/Add Health Bar to Enemy", true)]
    static bool ValidateAddHealthBarToEnemy()
    {
        return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Enemy>() != null;
    }
}

