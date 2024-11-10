using Microsoft.AspNetCore.Mvc;

public abstract class BaseController<T> : ControllerBase where T : BaseModel
{
    protected void SetCreateFields(T model, Guid userId)
    {
        model.CreatedAt = DateTime.Now;
        model.UpdatedAt = DateTime.Now;
        model.CreatedBy = userId;
        model.UpdatedBy = userId;
    }

    protected void SetUpdateFields(T model, Guid userId)
    {
        model.UpdatedAt = DateTime.Now;
        model.UpdatedBy = userId;
    }
}
