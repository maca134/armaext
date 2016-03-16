class CfgPatches {
	class armaext {
		version = "1";
		units[] = {};
		weapons[] = {};
		requiredVersion = "1";
		requiredAddons[] = {};
	};
};

class CfgFunctions {
	class ARMAEXT {
		class Core {
			file = "\armaext";
			class load {};
			class run {};
		};
	};
};
