using System;
using System.Collections;
using System.Collections.Generic;
using Common.IoC;
using GPUInstancer;
using UnityEngine;

public class GpuIstancerPool : Pool<GPUInstancerPrefab>
{
    private GPUInstancerPrefabManager _instancerManager;

    public GpuIstancerPool(GPUInstancerPrefab originalPrefab, int poolSize, Transform parent, GPUInstancerPrefabManager instancerManager, IIocContainer iocContainer = null) : base(originalPrefab, poolSize, parent, iocContainer)
    {
        _instancerManager = instancerManager;
        _instancerManager.RegisterPrefabInstanceList(this);
    }

    public override GPUInstancerPrefab GetItem()
    {
        var item = base.GetItem();

        return item;
    }

    protected override GPUInstancerPrefab SpawnNew()
    {
        GPUInstancerPrefab obj = base.SpawnNew();
        _instancerManager?.AddPrefabInstance(obj, true);

        return obj;
    }
    public override void ReturnItem(GPUInstancerPrefab item)
    {
        _instancerManager.DisableIntancingForInstance(item);
        base.ReturnItem(item);
    }
}
