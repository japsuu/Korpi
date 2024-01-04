﻿using BlockEngine.Client.ECS.Entities;

namespace BlockEngine.Client.ECS.Components;

public abstract class Component
{
    public bool IsEnabled { get; private set; }
    
    protected Entity Entity { get; private set; } = null!;
    
    
    protected Component() { }


    public void SetEntity(Entity entity)
    {
        Entity = entity;
    }


    public void Enable()
    {
        IsEnabled = true;
        OnEnable();
    }
    
    
    public void Update()
    {
        OnUpdate();
    }
    
    
    public void Draw()
    {
        OnDraw();
    }
    
    
    public void Disable()
    {
        IsEnabled = false;
        OnDisable();
    }
    
    
    public void Destroy()
    {
        OnDestroy();
    }


    protected virtual void OnEnable() { }
    
    protected virtual void OnUpdate() { }
    
    protected virtual void OnDraw() { }
    
    protected virtual void OnDisable() { }
    
    protected virtual void OnDestroy() { }
}