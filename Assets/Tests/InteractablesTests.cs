using NUnit.Framework;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.TestTools;   // LogAssert

public class InteractablesTests
{
    [SetUp]
    public void MuteLogs()
    {
        // por si algún script hace logs; no deberían romper tests
        LogAssert.ignoreFailingMessages = true;
    }

    [TearDown]
    public void Cleanup()
    {
        LogAssert.ignoreFailingMessages = false;
        foreach (var go in UnityEngine.Object.FindObjectsOfType<GameObject>())
            if (go.name.EndsWith("_Test"))
                UnityEngine.Object.DestroyImmediate(go);
    }

    // ----------- UTILIDADES ------------

    private static Type GetTypeByName(string name)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(name);
            if (t != null) return t;
        }
        return null;
    }

    // Crea GO + componente por nombre, asigna sharedMaterial y
    // INYECTA el Renderer/Color en los campos privados, SIN llamar Start()
    private static (GameObject go, MonoBehaviour comp, Renderer renderer)
        CreateComponentByName(string typeName)
    {
        var t = GetTypeByName(typeName);
        Assert.IsNotNull(t, $"No se encontró el tipo '{typeName}'. ¿El script está en Assets/Scripts (fuera de Tests)?");

        var go = new GameObject(typeName + "_Test");

        // Renderer + sharedMaterial para evitar warning por instanciar materiales
        var renderer = go.AddComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Standard"));
        renderer.sharedMaterial = mat;

        var comp = (MonoBehaviour)go.AddComponent(t);

        // Inyectamos el Renderer en cualquier campo privado de tipo Renderer (doorRenderer/leverRenderer/etc.)
        var fiRenderer = t.GetField("doorRenderer", BindingFlags.Instance | BindingFlags.NonPublic)
                      ?? t.GetField("leverRenderer", BindingFlags.Instance | BindingFlags.NonPublic);
        if (fiRenderer != null) fiRenderer.SetValue(comp, renderer);

        // Inyectamos originalColor si existe (evita tener que leerlo en Start())
        var fiOriginalColor = t.GetField("originalColor", BindingFlags.Instance | BindingFlags.NonPublic);
        if (fiOriginalColor != null) fiOriginalColor.SetValue(comp, renderer.sharedMaterial.color);

        // ¡OJO! No llamamos Start() para no ejecutar código que usa renderer.material en EditMode.
        return (go, comp, renderer);
    }

    private static void CallMethod(object obj, string methodName)
    {
        var m = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(m, $"{obj.GetType().Name} debería tener el método {methodName}().");
        m.Invoke(obj, null);
    }

    private static bool GetIsOpen(object obj)
    {
        var f = obj.GetType().GetField("isOpen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(f, $"{obj.GetType().Name} debe tener un campo 'isOpen'.");
        return (bool)f.GetValue(obj);
    }

    // ------------- TESTS (5) -------------

    [Test]
    public void Door_TogglesOpen_WhenInteracted()
    {
        var created = CreateComponentByName("Door");
        bool before = GetIsOpen(created.comp);

        CallMethod(created.comp, "Interact");
        bool after = GetIsOpen(created.comp);

        Assert.AreNotEqual(before, after, "Door.Interact() debe invertir 'isOpen'.");
    }

    [Test]
    public void SlidingDoor_TogglesOpen_WhenInteracted()
    {
        var created = CreateComponentByName("SlidingDoor");
        bool before = GetIsOpen(created.comp);

        CallMethod(created.comp, "Interact");
        bool after = GetIsOpen(created.comp);

        Assert.AreNotEqual(before, after, "SlidingDoor.Interact() debe invertir 'isOpen'.");
    }

    [Test]
    public void Palanc_TogglesOpen_WhenInteracted()
    {
        var created = CreateComponentByName("Palanc");
        bool before = GetIsOpen(created.comp);

        CallMethod(created.comp, "Interact");
        bool after = GetIsOpen(created.comp);

        Assert.AreNotEqual(before, after, "Palanc.Interact() debe invertir 'isOpen'.");
    }

    [Test]
    public void Interactable_ChangesColor_OnSelect()
    {
        var created = CreateComponentByName("Door");
        var rend = created.renderer;
        var before = rend.sharedMaterial.color;

        // Estos métodos en tus scripts usan renderer.material, pero como NO llamamos Start()
        // y usamos sharedMaterial, debería reducir/evitar warnings.
        CallMethod(created.comp, "OnSelect");
        var after = rend.sharedMaterial.color;

        Assert.AreNotEqual(before, after, "OnSelect debería cambiar el color del material.");
    }

    [Test]
    public void Interactable_RestoresColor_OnDeselect()
    {
        var created = CreateComponentByName("Door");
        var rend = created.renderer;
        var original = rend.sharedMaterial.color;

        CallMethod(created.comp, "OnSelect");
        CallMethod(created.comp, "OnDeselect");

        Assert.AreEqual(original, rend.sharedMaterial.color, "OnDeselect debería restaurar el color original.");
    }
}
