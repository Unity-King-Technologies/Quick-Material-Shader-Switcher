using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class QuickMaterialSwitcherWindow : EditorWindow
{
    private List<Material> allMaterials = new List<Material>();
    private List<Material> filteredMaterials = new List<Material>();
    private List<Material> favorites = new List<Material>();
    private string searchQuery = "";
    private ScrollView materialScrollView;
    private TextField searchField;
    private VisualElement root;
    private bool showFavoritesOnly = false;

    // Innovative features
    private Material previewMaterial;
    private bool compareMode = false;
    private VisualElement compareContainer;

    [MenuItem("Tools/Quick Material / Shader Switcher")]
    static void Init()
    {
        QuickMaterialSwitcherWindow window = (QuickMaterialSwitcherWindow)EditorWindow.GetWindow(typeof(QuickMaterialSwitcherWindow));
        window.titleContent = new GUIContent("Material Switcher");
        window.Show();
    }

    void OnEnable()
    {
        LoadMaterials();
        LoadFavorites();
        CreateGUI();
    }

    void OnDisable()
    {
        SaveFavorites();
    }

    void CreateGUI()
    {
        root = rootVisualElement;

        var toolbar = new Toolbar();
        root.Add(toolbar);

        searchField = new TextField();
        searchField.placeholderText = "Search materials...";
        searchField.RegisterValueChangedCallback(evt => FilterMaterials(evt.newValue));
        toolbar.Add(searchField);

        var favoritesToggle = new Toggle("Favorites Only");
        favoritesToggle.RegisterValueChangedCallback(evt =>
        {
            showFavoritesOnly = evt.newValue;
            FilterMaterials(searchQuery);
        });
        toolbar.Add(favoritesToggle);

        var compareToggle = new Toggle("Compare Mode");
        compareToggle.RegisterValueChangedCallback(evt =>
        {
            compareMode = evt.newValue;
            ToggleCompareMode();
        });
        toolbar.Add(compareToggle);

        var refreshButton = new Button(() => LoadMaterials()) { text = "Refresh" };
        toolbar.Add(refreshButton);

        // Shader Tools Section
        var shaderTools = new Foldout() { text = "Shader Tools" };
        root.Add(shaderTools);

        var shaderDropdown = new DropdownField("Select Shader");
        shaderDropdown.choices = ShaderSwitcherUtility.GetAllShaders().Select(s => s.name).ToList();
        shaderTools.Add(shaderDropdown);

        var switchShaderButton = new Button(() => SwitchShaderOnSelected(shaderDropdown.value)) { text = "Switch Shader on Selected Materials" };
        shaderTools.Add(switchShaderButton);

        // Batch Replace Section
        var batchReplace = new Foldout() { text = "Batch Replace" };
        root.Add(batchReplace);

        var oldMaterialField = new ObjectField("Old Material") { objectType = typeof(Material) };
        batchReplace.Add(oldMaterialField);

        var newMaterialField = new ObjectField("New Material") { objectType = typeof(Material) };
        batchReplace.Add(newMaterialField);

        var replaceButton = new Button(() => BatchReplaceMaterials((Material)oldMaterialField.value, (Material)newMaterialField.value)) { text = "Replace All" };
        batchReplace.Add(replaceButton);

        // Quick Actions
        var quickActions = new Foldout() { text = "Quick Actions" };
        root.Add(quickActions);

        var randomAssignButton = new Button(() => RandomAssignMaterials()) { text = "Random Assign to Selected" };
        quickActions.Add(randomAssignButton);

        // Material list
        materialScrollView = new ScrollView();
        root.Add(materialScrollView);

        // Compare container (hidden by default)
        compareContainer = new VisualElement();
        compareContainer.style.display = DisplayStyle.None;
        compareContainer.style.flexDirection = FlexDirection.Row;
        compareContainer.style.height = 200;
        root.Add(compareContainer);

        var leftPreview = new VisualElement();
        leftPreview.style.flexGrow = 1;
        leftPreview.style.backgroundColor = Color.gray;
        compareContainer.Add(leftPreview);

        var rightPreview = new VisualElement();
        rightPreview.style.flexGrow = 1;
        rightPreview.style.backgroundColor = Color.gray;
        compareContainer.Add(rightPreview);

        PopulateMaterialList();
    }

    void LoadMaterials()
    {
        allMaterials.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                allMaterials.Add(mat);
            }
        }
        FilterMaterials(searchQuery);
    }

    void FilterMaterials(string query)
    {
        searchQuery = query;
        filteredMaterials = allMaterials.Where(mat =>
            (string.IsNullOrEmpty(query) || mat.name.ToLower().Contains(query.ToLower())) &&
            (!showFavoritesOnly || favorites.Contains(mat))
        ).ToList();
        PopulateMaterialList();
    }

    void PopulateMaterialList()
    {
        materialScrollView.Clear();
        foreach (var material in filteredMaterials)
        {
            var materialItem = CreateMaterialItem(material);
            materialScrollView.Add(materialItem);
        }
    }

    VisualElement CreateMaterialItem(Material material)
    {
        var item = new VisualElement();
        item.style.flexDirection = FlexDirection.Row;
        item.style.marginBottom = 5;
        item.style.padding = 5;
        item.style.borderBottomWidth = 1;
        item.style.borderBottomColor = Color.gray;

        // Thumbnail
        var thumbnail = new VisualElement();
        thumbnail.style.width = 50;
        thumbnail.style.height = 50;
        thumbnail.style.backgroundImage = AssetPreview.GetAssetPreview(material);
        thumbnail.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
        item.Add(thumbnail);

        // Info
        var info = new VisualElement();
        info.style.flexGrow = 1;
        info.style.flexDirection = FlexDirection.Column;

        var nameLabel = new Label(material.name);
        nameLabel.style.fontSize = 14;
        nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        info.Add(nameLabel);

        var shaderLabel = new Label(material.shader.name);
        shaderLabel.style.fontSize = 12;
        shaderLabel.style.color = Color.gray;
        info.Add(shaderLabel);

        item.Add(info);

        // Buttons
        var buttons = new VisualElement();
        buttons.style.flexDirection = FlexDirection.Row;

        var assignButton = new Button(() => AssignMaterial(material)) { text = "Assign" };
        buttons.Add(assignButton);

        var favoriteButton = new Button(() => ToggleFavorite(material)) { text = favorites.Contains(material) ? "★" : "☆" };
        buttons.Add(favoriteButton);

        var compareButton = new Button(() => SetCompareMaterial(material)) { text = "Compare" };
        buttons.Add(compareButton);

        item.Add(buttons);

        // Drag and drop
        item.RegisterCallback<MouseDownEvent>(evt =>
        {
            if (evt.button == 0) // Left click
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[] { material };
                DragAndDrop.StartDrag("Dragging Material");
            }
        });

        return item;
    }

    void AssignMaterial(Material material)
    {
        var selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0) return;

        Undo.RecordObjects(selectedObjects, "Assign Material");

        foreach (var obj in selectedObjects)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.sharedMaterial = material;
            }
        }
    }

    void ToggleFavorite(Material material)
    {
        if (favorites.Contains(material))
        {
            favorites.Remove(material);
        }
        else
        {
            favorites.Add(material);
        }
        PopulateMaterialList();
    }

    void SetCompareMaterial(Material material)
    {
        previewMaterial = material;
        UpdateCompareView();
    }

    void ToggleCompareMode()
    {
        compareContainer.style.display = compareMode ? DisplayStyle.Flex : DisplayStyle.None;
        if (compareMode)
        {
            UpdateCompareView();
        }
    }

    void UpdateCompareView()
    {
        if (!compareMode || previewMaterial == null) return;

        // This would need more implementation for actual preview rendering
        // For simplicity, just show names
        var leftLabel = compareContainer[0].Q<Label>();
        if (leftLabel == null)
        {
            leftLabel = new Label();
            compareContainer[0].Add(leftLabel);
        }
        leftLabel.text = previewMaterial.name;

        // Right side could show current material or another
    }

    void LoadFavorites()
    {
        string data = EditorPrefs.GetString("QuickMaterialSwitcher_Favorites", "");
        if (!string.IsNullOrEmpty(data))
        {
            string[] paths = data.Split(';');
            foreach (string path in paths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (mat != null)
                    {
                        favorites.Add(mat);
                    }
                }
            }
        }
    }

    void SaveFavorites()
    {
        string data = string.Join(";", favorites.Select(mat => AssetDatabase.GetAssetPath(mat)));
        EditorPrefs.SetString("QuickMaterialSwitcher_Favorites", data);
    }

    void SwitchShaderOnSelected(string shaderName)
    {
        var shader = ShaderSwitcherUtility.GetAllShaders().FirstOrDefault(s => s.name == shaderName);
        if (shader == null) return;

        var selectedMaterials = Selection.objects.OfType<Material>().ToList();
        if (selectedMaterials.Count == 0)
        {
            // If no materials selected, switch on materials of selected objects
            var selectedObjects = Selection.gameObjects;
            var materials = new HashSet<Material>();
            foreach (var obj in selectedObjects)
            {
                var renderers = obj.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    if (r.sharedMaterial != null)
                        materials.Add(r.sharedMaterial);
                }
            }
            selectedMaterials = materials.ToList();
        }

        ShaderSwitcherUtility.BatchSwitchShaders(selectedMaterials, shader);
    }

    void BatchReplaceMaterials(Material oldMat, Material newMat)
    {
        if (oldMat == null || newMat == null) return;

        // Find all renderers using oldMat and replace
        var allRenderers = FindObjectsOfType<Renderer>();
        Undo.RecordObjects(allRenderers, "Batch Replace Materials");

        foreach (var renderer in allRenderers)
        {
            if (renderer.sharedMaterial == oldMat)
            {
                renderer.sharedMaterial = newMat;
            }
        }
    }

    void RandomAssignMaterials()
    {
        var selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0 || allMaterials.Count == 0) return;

        Undo.RecordObjects(selectedObjects, "Random Assign Materials");

        var random = new System.Random();
        foreach (var obj in selectedObjects)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                var randomMat = allMaterials[random.Next(allMaterials.Count)];
                renderer.sharedMaterial = randomMat;
            }
        }
    }

    // Keyboard shortcuts
    void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.F && Event.current.control)
            {
                searchField.Focus();
                Event.current.Use();
            }
            else if (Event.current.keyCode >= KeyCode.Alpha1 && Event.current.keyCode <= KeyCode.Alpha9)
            {
                int index = Event.current.keyCode - KeyCode.Alpha1;
                if (index < filteredMaterials.Count)
                {
                    AssignMaterial(filteredMaterials[index]);
                    Event.current.Use();
                }
            }
            else if (Event.current.keyCode == KeyCode.R && Event.current.control)
            {
                RandomAssignMaterials();
                Event.current.Use();
            }
        }
    }
}
