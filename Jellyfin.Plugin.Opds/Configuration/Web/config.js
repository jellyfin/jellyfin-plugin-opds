export default function (view){
    
    const Plugin = {
        pluginId: 'F30880AE-3365-449E-B9E6-BF133C8401B0',
        chkAllowAnonymous: document.querySelector('#chkAllowAnonymous'),
        libraryWrapper: document.querySelector('#library-wrapper'),
        libraryTemplate: document.querySelector('#library-template'),
        
        init: async function(){
            const config = await window.ApiClient.getPluginConfiguration(Plugin.pluginId);
            const virtualFolders = await window.ApiClient.getVirtualFolders();
            for(const folder of virtualFolders){
                const template = Plugin.libraryTemplate.cloneNode(true).content;
                const name = template.querySelector('[data-name=libraryName]');
                const value = template.querySelector('[data-name=chkLibrary]');
                
                name.innerText = folder.Name;
                value.dataset.value = folder.ItemId;
                value.checked = config.BookLibraries.includes(folder.ItemId);
                
                Plugin.libraryWrapper.appendChild(template);
            }            
            
            Plugin.chkAllowAnonymous.checked = config.AllowAnonymousAccess;

            document.querySelector("#saveConfig").addEventListener("click", Plugin.saveConfig);
        },
        getLibraries: function(){
            const checkboxes = Plugin.libraryWrapper.querySelectorAll('[data-name=chkLibrary]:checked');
            const libraries = [];
            for(const checkbox of checkboxes) {
                libraries.push(checkbox.dataset.value);
            }
            
            return libraries;
        },
        saveConfig: function(e){
            e.preventDefault();
            
            const config = {};
            config.AllowAnonymousAccess = Plugin.chkAllowAnonymous.checked;
            config.BookLibraries = Plugin.getLibraries();

            window.ApiClient.updatePluginConfiguration(Plugin.pluginId, config).then(Dashboard.processPluginConfigurationUpdateResult);
        }
    }
    
    view.addEventListener("viewshow", async function(){
        await Plugin.init();
    });
}
