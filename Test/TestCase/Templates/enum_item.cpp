    public enum class {{ enum.unscoped_name }}
    { {% for dict_field in enum.fields %} {% assign field = dict_field.Value %}
        {{ field.name }} = {{ field.constant }}, {% endfor %}
    }