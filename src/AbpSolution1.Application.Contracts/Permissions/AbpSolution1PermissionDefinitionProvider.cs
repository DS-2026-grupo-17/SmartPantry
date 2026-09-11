using AbpSolution1.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace AbpSolution1.Permissions;

public class AbpSolution1PermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(AbpSolution1Permissions.GroupName);

        var booksPermission = myGroup.AddPermission(AbpSolution1Permissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(AbpSolution1Permissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(AbpSolution1Permissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(AbpSolution1Permissions.Books.Delete, L("Permission:Books.Delete"));

        var authorsPermission = myGroup.AddPermission(AbpSolution1Permissions.Authors.Default, L("Permission:Authors"));
        authorsPermission.AddChild(AbpSolution1Permissions.Authors.Create, L("Permission:Authors.Create"));
        authorsPermission.AddChild(AbpSolution1Permissions.Authors.Edit, L("Permission:Authors.Edit"));
        authorsPermission.AddChild(AbpSolution1Permissions.Authors.Delete, L("Permission:Authors.Delete"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(AbpSolution1Permissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AbpSolution1Resource>(name);
    }
}
